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
using System.Reflection;//delete me if after project presentation

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
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly Timer _timer;
        private volatile bool _updatingPlayerStats = false;

        public int playCount = 0;  //nextplay + 1. starts at 0 so -1 +1 = 0
        ReadJSONDatafromNFL r;
        public List<NFLPlayer> livePlayerList = new List<NFLPlayer>();
        public List<int> liveUpdateList = new List<int>();

        private NFLStatsUpdate(IHubConnectionContext<dynamic> clients) {

            Clients = clients;

            _players.Clear();
            using (var db = new FF()) {
                livePlayerList = db.NFLPlayer.ToList();
            }

            foreach (NFLPlayer n in livePlayerList) {
                Type m1 = n.PassingStats.GetType();
                Type m2 = n.RushingStats.GetType();
                Type m3 = n.ReceivingStats.GetType();
                Type m4 = n.KickingStats.GetType();
                Type m5 = n.FumbleStats.GetType();
                PropertyInfo[] myProps1 = m1.GetProperties();
                PropertyInfo[] myProps2 = m2.GetProperties();
                PropertyInfo[] myProps3 = m3.GetProperties();
                PropertyInfo[] myProps4 = m4.GetProperties();
                PropertyInfo[] myProps5 = m5.GetProperties();
                foreach (PropertyInfo p in myProps1)
                {
                    p.SetValue(n.PassingStats, 0);
                }
                foreach (PropertyInfo p in myProps2)
                {
                    p.SetValue(n.RushingStats, 0);
                }
                foreach (PropertyInfo p in myProps3)
                {
                    p.SetValue(n.ReceivingStats, 0);
                }
                foreach (PropertyInfo p in myProps4)
                {
                    p.SetValue(n.KickingStats, 0);
                }
                foreach (PropertyInfo p in myProps5)
                {
                    p.SetValue(n.FumbleStats, 0);
                }
                _players.TryAdd(n.id, n);
            }

            livePlayerList.Clear();     //empty list

            //make a list of players that got updated and send to updatePlayer?
            //livePlayerList.ForEach(player => _players.TryAdd(player.id, player));


            r = new ReadJSONDatafromNFL("2015101200_gtd.json");
            /*****
             * NOTE- For stupid demo I have to null/comment out stats in ReadJSON
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

        public bool ProcessPlay(PlaysVM currPlay) {
            NFLPlayer n = new NFLPlayer();

            foreach (PlayersVM p in currPlay.Players) {

                if (_players.TryGetValue(p.id, out n)) {
                    //do update on player in allLive dict
                    NFLPlayer copy = new NFLPlayer();
                    copy = UpdateStat(p, n, currPlay.Note);
                    //update dict value
                    _players[p.id] = copy;
                    liveUpdateList.Add(copy.id);
                }
            }
            return true;
        }

        private void UpdatePlayerStats(object state)
        {
            lock (_updatePlayersStatsLock)
            {
                if (!_updatingPlayerStats)
                {
                    var f = r.NextPlay(playCount);  //go to next play
                    _updatingPlayerStats = ProcessPlay(f);
                    BrodcastPlay(f.Desc);
                    //go through list of sent players and update accordingly
                    //_updatingPlayerStats = true;

                    if (liveUpdateList.Count != 0)
                    {
                        foreach (var playerid in liveUpdateList)
                        {
                            //only have to do players in game though may go through home and awayplayers?
                            //find id in _players
                            NFLPlayer updatedP;

                            if (_players.TryGetValue(playerid, out updatedP))
                            {
                                if (TryUpdateHomePlayerPoint(updatedP))
                                   BroadcastPlayers(updatedP);                            
                                else if (TryUpdateAwayPlayerPoint(updatedP))
                                    BroadcastPlayers(updatedP);
                                
                                                          
                            }
                            else
                                throw new Exception("Something went wrong in updateplayerstats _players.TryGetValue!");
                        }
                    }
                 

                    //foreach (var player in _homePlayers.Values)
                    //{

                    //}

                    //foreach (var player in _awayPlayers.Values) {
                    //    if (TryUpdatePlayerPoint(player)) {
                    //        BroadcastPlayers(player);
                    //    }
                    //}

                    _updatingPlayerStats = false;
                    liveUpdateList.Clear(); //clear for next run
                    ++playCount;
                }
            }
        }

        private bool TryUpdateHomePlayerPoint(NFLPlayer player)
        {
            NFLPlayer homePlayer;
 
            if (_homePlayers.TryGetValue(player.id, out homePlayer)) {
                _homePlayers[player.id] = player;
                
                return true;
            }

             return false;
          
        }

        private bool TryUpdateAwayPlayerPoint(NFLPlayer player)
        {
            NFLPlayer awayPlayer;

            if (_awayPlayers.TryGetValue(player.id, out awayPlayer))
            {
                _awayPlayers[player.id] = player;

                return true;
            }

            return false;

        }

        private void BroadcastPlayers(NFLPlayer player)
        {
            Clients.All.updatePlayers(player);
        }

        private void BrodcastPlay(string play)
        {
            Clients.All.updatePlay(play);
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

        /*
         * Helper functions for NFLPlayerData 
         */

        public NFLPlayer UpdateStat(PlayersVM fromPlay, NFLPlayer fromList, string note)
        {
            //Take fromPlayer statID and find stat
            //Add in fumbles later, confusing, 
            //whenever a pass, 4 with no playerId comes up, 7 with no playerID -> these will get screened out because no playerID   figure out later
            //Penalties could fuck up yardage

            switch (fromPlay.StatId)
            {
                case 10:
                    //rush yds
                    fromList.RushingStats.RushAtt += 1;
                    fromList.RushingStats.RushYds += fromPlay.Yards;

                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //RushLng Don't add in right now

                    break;
                case 11:
                    //rushTDyds
                    fromList.RushingStats.RushAtt += 1;
                    fromList.RushingStats.RushYds += fromPlay.Yards;

                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //RushLngTD Don't add in right now
                    if (note == "TD") {
                        fromList.RushingStats.RushTds += 1;
                        fromList.currentPts += 6;
                    }
                    else
                        throw new Exception("Has RushingTDYards but note is not TD <- check play");
                    break;
                case 14:
                    //passInc
                    fromList.PassingStats.PassAtt += 1;
                    break;
                case 15:
                    //PassYrds
                    fromList.PassingStats.PassYds += fromPlay.Yards;
                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    break;
                case 16:
                    //PassYrdsTD
                    fromList.PassingStats.PassYds += fromPlay.Yards;
                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));

                    if (note == "TD")
                    {
                        fromList.PassingStats.PassTds += 1;
                        fromList.currentPts += 6;
                    }
                    else
                        throw new Exception("Has PassTDYards but note is not TD <- check play");
                    break;
                case 19:
                    //PassINT
                    if (note == "INT")
                    {
                        fromList.PassingStats.PassInts += 1;
                        fromList.currentPts += -2;
                    }
                    //add int Exception it triggers on chargersvssteelers

                    break;
                case 20:
                    //QBSack
                    fromList.PassingStats.PassAtt += 1;
                    //sack yardage goes in to team total but qb indv stats do not change

                    break;
                case 21:
                    //RecYds
                    fromList.ReceivingStats.Rec += 1;
                    fromList.ReceivingStats.RecYds += fromPlay.Yards;

                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //add in RecLng Later
                    break;
                case 22:
                    //RecYrdsTD
                    fromList.ReceivingStats.Rec += 1;
                    fromList.ReceivingStats.RecYds += fromPlay.Yards;
                    fromList.ReceivingStats.RecTds += 1;

                    fromList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //add in RecLngTD later
                    break;
                case 69:
                    //FGMiss
                    if (note == "FGM")
                    {
                        fromList.KickingStats.Fga += 1;
                        fromList.KickingStats.Fgyds += fromPlay.Yards;
                    }
                    else
                        throw new Exception("statID says FGM, but note doesn't <- check play");
                    break;
                case 70:
                    //FG
                    if (note == "FG")
                    {
                        fromList.KickingStats.Fga += 1;
                        fromList.KickingStats.Fgm += 1;
                        fromList.KickingStats.Fgyds += fromPlay.Yards;

                        fromList.currentPts += Convert.ToDecimal(Math.Floor(fromPlay.Yards / 10.00));
                    }
                    else
                        throw new Exception("statID says FG, but note doesn't <- check play");
                    break;
                case 71:
                    //Guessing this is XPMISS
                    if (note == "XPM")
                        throw new Exception("THIS IS XPMISSED");
                    break;
                case 72:
                    //XP
                    if (note == "XP")
                    {
                        fromList.KickingStats.Xpa += 1;
                        fromList.KickingStats.Xpmade += 1;
                        fromList.KickingStats.Xptot += 1;

                        fromList.currentPts += 1;
                    }
                    else
                        throw new Exception("statID says XP, but note doesn't <- check play");
                    break;
                case 111:
                    //yrds thrown cmp
                    fromList.PassingStats.PassAtt += 1;
                    fromList.PassingStats.PassCmp += 1;
                    break;
                case 112:
                    //yrds thrown inc
                    fromList.PassingStats.PassAtt += 1;
                    break;
                case 113:
                    //YAC (For AdvStats)
                    break;
                case 115:
                    //PassTarget
                    fromList.ReceivingStats.RecTrg += 1;
                    break;
                default:
                    //write out to file with plays that don't get flagged later after adding in fumbles, ko's, penalty, etc.
                    break;
            }



            return fromList; //appease debugger
        }
    }
}