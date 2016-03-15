﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//added using
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WebApplication1.Models;
using WebApplication1.DAL;

namespace WebApplication1.Hubs
{
    public class NFLStatsUpdate
    {
        // Singleton instance
        private readonly static Lazy<NFLStatsUpdate> _instance = new Lazy<NFLStatsUpdate>(
            () => new NFLStatsUpdate(GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>().Clients));

        private readonly ConcurrentDictionary<int, NFLPlayer> _players = new ConcurrentDictionary<int, NFLPlayer>();

        private readonly object _updatePlayersStatsLock = new object();

        //1000 ms = 1 sec
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(3000);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly Timer _timer;
        private volatile bool _updatingPlayerStats = false;

        private NFLStatsUpdate(IHubConnectionContext<dynamic> clients)
        {
            using (var FFContext = new FF())
            {
                var PlayersList = FFContext.NFLPlayer.ToList();

                Clients = clients;

                _players.Clear();

                PlayersList.ForEach(player => _players.TryAdd(player.id, player));

                _timer = new Timer(UpdatePlayerStats, null, _updateInterval, _updateInterval);
            }

        }

        public static NFLStatsUpdate Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }
        

        public IEnumerable<NFLPlayer> GetAllPlayers()
        {
            return _players.Values;
        }

        private void UpdatePlayerStats(object state)
        {
            lock (_updatePlayersStatsLock)
            {
                if (!_updatingPlayerStats)
                {
                    _updatingPlayerStats = true;

                    foreach (var player in _players.Values)
                    {
                        if (TryUpdatePlayerPoint(player))
                        {
                            BroadcastPlayers(player);
                        }
                    }

                    _updatingPlayerStats = false;
                }
            }
        }

        private bool TryUpdatePlayerPoint(NFLPlayer player)
        {
            //if isLive == true && currentpoint != lastpoint
            if (player.id == 22942 || player.id == 21547) 
                player.currentPts += 3;
            
            else if (player.id == 32144) 
                player.currentPts += 2;
            
            else  
                return false; 

            return true;
        }

        private void BroadcastPlayers(NFLPlayer player)
        {
            Clients.All.updatePlayers(player);
        }

    }
}