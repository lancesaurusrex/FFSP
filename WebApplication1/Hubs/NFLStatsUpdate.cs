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
        // Singleton instance
        private readonly static Lazy<NFLStatsUpdate> _instance = new Lazy<NFLStatsUpdate>(
            () => new NFLStatsUpdate(GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>().Clients));

        private readonly ConcurrentDictionary<int, NFLPlayer> _players = new ConcurrentDictionary<int, NFLPlayer>();
        private readonly ConcurrentDictionary<int, NFLPlayer> _homePlayers = new ConcurrentDictionary<int, NFLPlayer>();
        private readonly ConcurrentDictionary<int, NFLPlayer> _awayPlayers = new ConcurrentDictionary<int, NFLPlayer>();

        private readonly object _updatePlayersStatsLock = new object();

        //1000 ms = 1 sec
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(5000);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly Timer _timer;
        private volatile bool _updatingPlayerStats = false;

        public int playCount = 0;
        ReadJSONDatafromNFL r;
        public List<NFLPlayer> livePlayerList = new List<NFLPlayer>();

        private NFLStatsUpdate(IHubConnectionContext<dynamic> clients) {

            Clients = clients;

            _players.Clear();
            using (var db = new FF()) {
                livePlayerList = db.NFLPlayer.ToList();
            }

            foreach (NFLPlayer plyr in livePlayerList) {
                _players.TryAdd(plyr.id, plyr);
            }

            livePlayerList.Clear();
            //make a list of players that got updated and send to updatePlayer?
            //livePlayerList.ForEach(player => _players.TryAdd(player.id, player));


            r = new ReadJSONDatafromNFL("2015101200_gtd.json");
            /*****
             * NOTE- For stupid demo I have to null out stats in ReadJSON
             *****/
            r.QuickParseAfterLive();

            _timer = new Timer(UpdatePlayerStats, null, _updateInterval, _updateInterval);
        }

  
        public static NFLStatsUpdate Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }
        

        public IEnumerable<NFLPlayer> GetAllLivePlayers(int gid)
        {
            return _players.Values;
        }

        public IEnumerable<NFLPlayer> GetAllHomePlayers(int gid) { 
            var homeID = GetHomeTeamIDFromGameID(gid);
            var homePlayersID = GetAllPlayersIDOnTeam(homeID);
            var homePlayersList = GetAllPlayersFromPlayersID(homePlayersID);
            homePlayersList.ForEach(player => _homePlayers.TryAdd(player.id, player));

            return _homePlayers.Values;
        }
        
        public IEnumerable<NFLPlayer> GetAllAwayPlayers(int gid) {
            var awayID = GetAwayTeamIDFromGameID(gid);
            var awayPlayersID = GetAllPlayersIDOnTeam(awayID);
            var awayPlayersList = GetAllPlayersFromPlayersID(awayPlayersID);
            awayPlayersList.ForEach(player => _awayPlayers.TryAdd(player.id, player));

            return _awayPlayers.Values;
        }

        public void ProcessPlay(PlaysVM currPlay) {
            NFLPlayer n;

            foreach (PlayersVM p in currPlay.Players) {

                if (_players.TryGetValue(p.id, out n)) {
                    //do update on player in allLive dict
                    n = UpdateStat(p, n, currPlay.Note);
                    //update dict value
                    _players[p.id] = n;
                }
            }
        }

        public NFLPlayer UpdateStat(PlayersVM fromPlay, NFLPlayer fromList, string note) {
            //Take fromPlayer statID and find stat
            switch (fromPlay.StatId) {
                case 10:
                    //rush yds
                    fromList.RushingStats.RushYds += fromPlay.Yards;
                    break;
                case 11:
                    //rushTDyds
                    //if (note != "null")
                    break;
                case 14:
                    break;
                case 15:
                    
                    break;
                case 16:
                    break;
                case 20:
                    break;
                case 21:
                    break;
                case 70:
                    break;
                case 72:
                    break;
                default:
                    break;
            }
            return fromList; //appease debugger
        }
        private void UpdatePlayerStats(object state)
        {
            lock (_updatePlayersStatsLock)
            {
                if (!_updatingPlayerStats)
                {
                    var f = r.NextPlay(playCount);  //go to next play
                    ProcessPlay(f);
                    //go through list of sent players and update accordingly
                    _updatingPlayerStats = true;

                    foreach (var player in _homePlayers.Values)
                    {
                        if (TryUpdatePlayerPoint(player))
                        {
                            BroadcastPlayers(player);
                        }
                    }

                    foreach (var player in _awayPlayers.Values) {
                        if (TryUpdatePlayerPoint(player)) {
                            BroadcastPlayers(player);
                        }
                    }

                    _updatingPlayerStats = false;
                    ++playCount;
                }
            }
        }

        private bool TryUpdatePlayerPoint(NFLPlayer player)
        {
            //if isLive == true && currentpoint != lastpoint
            if (player.id == 24242 || player.id == 20245) 
                player.currentPts += 3;
            
            else if (player.id == 21547) 
                player.currentPts += 2;
            
            else  
                return false; 

            return true;
        }

        private void BroadcastPlayers(NFLPlayer player)
        {
            Clients.All.updatePlayers(player);
        }

        /* Warning - Database functions ahead.  Proceed with caution.  The DB is a real bitch.
         */ 

        //could do this is GetAllPlayersonTeam but want to split up due to home/away (2teams) easier to debug
        public int GetHomeTeamIDFromGameID(int GameID) {
            
            FFGame currentGame;
            using (var FFContext = new FF()) {

                currentGame = FFContext.FFGameDB.Find(GameID);
            }
            return (int)currentGame.HomeTeamID;
        }

        //could do this is GetAllPlayersonTeam but want to split up due to home/away (2teams) easier to debug
        public int GetAwayTeamIDFromGameID(int GameID) {
            
            FFGame currentGame;
            using (var FFContext = new FF()) {

                currentGame = FFContext.FFGameDB.Find(GameID);
            }
            return (int)currentGame.VisTeamID;
        }

        //Uses TeamController function to getallplayersIDonteam with teamID
        //In-TeamID Out-ints of NFLPlayersID on teamID
        public List<int> GetAllPlayersIDOnTeam(int TeamID) {
            List<int> FindPlayersOnTeam;
            using (var FFContext = new FF()) {

                //pulling from NFLPlayerTeam DB, which are not NFLPlayer.
                var temp = (from p in FFContext.FFTeamNFLPlayer where p.TeamID == TeamID select p.PlayerID);
                FindPlayersOnTeam = temp.ToList();

            }
            return FindPlayersOnTeam;
        }

        //uses PlayersIDS to get List NFLPlayer objects
        //in-Ienum<int> PlayersID out-List<NFLPlayer>
        public List<NFLPlayer> GetAllPlayersFromPlayersID(IEnumerable<int> PlayersID) {
            List<NFLPlayer> PlayersOnTeamCol = new List<NFLPlayer>();

            using (var FFContext = new FF()) {

                foreach (var playerID in PlayersID) {
                    var pl = FFContext.NFLPlayer.Find(playerID);
                    PlayersOnTeamCol.Add(pl);
                }
            }

            return PlayersOnTeamCol;
        }

    }
}