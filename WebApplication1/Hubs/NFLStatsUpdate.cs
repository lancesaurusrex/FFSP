using System;
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
        IHubConnectionContext<dynamic> context = GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>();
        // Singleton instance
        private readonly static Lazy<NFLStatsUpdate> _instance = new Lazy<NFLStatsUpdate>(
            () => new NFLStatsUpdate(GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>().Clients));

        private readonly ConcurrentDictionary<int, NFLPlayer> _players = new ConcurrentDictionary<int, NFLPlayer>();

        private readonly object _updatePlayersStatsLock = new object();

        //stats can go up or down by a percentage of this factor on each change, might not need this
        private readonly double _statsPercent = .01;
        //1000 ms = 1 sec
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);
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
                        if (TryUpdateStockPrice(player))
                        {
                            BroadcastStockPrice(player);
                        }
                    }

                    _updatingPlayerStats = false;
                }
            }
        }

        private bool TryUpdateStockPrice(NFLPlayer player)
        {
            //Do something here?

            return true;
        }

        private void BroadcastPlayers(NFLPlayer player)
        {
            Clients.All.updatePlayers(player);
        }

    }
}