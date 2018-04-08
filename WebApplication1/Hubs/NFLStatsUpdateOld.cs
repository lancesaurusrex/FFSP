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
using System.Reflection;

//namespace WebApplication1.Hubs2 {
    public enum StatsState {
        Closed,
        Open
    }

    public class NFLStatsUpdateOld {
        // Singleton instance
        //private readonly static Lazy<NFLStatsUpdate> _instance = new Lazy<NFLStatsUpdate>(
        //    () => new NFLStatsUpdate(GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>().Clients));

        private readonly ConcurrentDictionary<int, StatsYearWeek> _playersStats = new ConcurrentDictionary<int, StatsYearWeek>();   //main pool of players, this gets updated first and feeds home/awayPlayers
        private readonly ConcurrentDictionary<int, StatsYearWeek> _homePlayers = new ConcurrentDictionary<int, StatsYearWeek>();
        private readonly ConcurrentDictionary<int, StatsYearWeek> _awayPlayers = new ConcurrentDictionary<int, StatsYearWeek>();

        private readonly object _updatePlayersStatsLock = new object();

        //1000 ms = 1 sec
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(200);

        //private readonly Timer _timer;
        private volatile bool _updatingPlayerStats = false;

        public int playCount = 0;  //nextplay + 1. starts at 0 so -1 +1 = 0
        public int pc2 = 0;
        ReadJSONDatafromNFL r;
        ReadJSONDatafromNFL r2;
        public List<NFLPlayer> livePlayerList = new List<NFLPlayer>();      //temp list for init
        public List<StatsYearWeek> liveStatsList = new List<StatsYearWeek>();
        public List<int> liveUpdateList = new List<int>();  //list of players from the current play
        public bool gameOver = false;
        public int gameID = new int();  //will pass in and use all over 
        public int currentWeekServ = 1;
        public const int currentYear = 2016;

        //StatsState - will stop
        private volatile StatsState _statsState;
        private readonly object _statsStateLock = new object();

        public StatsState StatsState {
            get { return _statsState; }
            private set { _statsState = value; }
        }

        private NFLStatsUpdateOld(IHubConnectionContext<dynamic> clients) {

            Clients = clients;

            _playersStats.Clear();

            IQueryable<NFLGame> currentWeekSchedule;

            using (var db = new FF()) 
                livePlayerList = db.NFLPlayer.ToList();
            
            foreach (NFLPlayer n in livePlayerList) {
                //Creating a new StatsYearWeek to track Stats with and connect with NFLPlayer
                StatsYearWeek s = new StatsYearWeek();
                SetSYWToZero(s);
                //Remember im doing this live not for stupid class demo, so ill need to make statid off of player
                //will need NOTregular check to avoid duplicate keys, will need to make week seperate higher number for preseason 110,111,112...?
                s.PlayerID = n.id;
                s.Year = currentYear;     //passed in from where?
                s.Week = currentWeekServ;     //passed in from where?
                s.currentPts = 0;
                string statID = (s.Year.ToString() + s.Week.ToString() + s.PlayerID).ToString();   //YearWeekPlayerID is StatID
                s.id = Convert.ToInt32(statID);

                liveStatsList.Add(s);   //list  
                _playersStats.TryAdd(s.id, s);  //dict
            }

            livePlayerList.Clear();     //empty list

            //make a list of players that got updated and send to updatePlayer?
            //livePlayerList.ForEach(player => _players.TryAdd(player.id, player));

            r = new ReadJSONDatafromNFL("2015101200_gtd.json", "2015101200");
            r2 = new ReadJSONDatafromNFL("2015101108_gtd.json", "2015101108");
            /*****
             * NOTE- For stupid demo I have to null/comment out stats in ReadJSON
             *****/
            r.QuickParseAfterLive();
            r2.QuickParseAfterLive();
            //if timer needed
            //_timer = new Timer(UpdatePlayerStats, null, _updateInterval, _updateInterval);
        }

        //public static NFLStatsUpdateOld Instance {
        //    get {
        //        return _instance.Value;
        //    }
        //}

        private IHubConnectionContext<dynamic> Clients { get; set; }


        public IEnumerable<StatsYearWeek> GetAllLivePlayers(int gid) {
            return _playersStats.Values;
        }

        public IEnumerable<StatsYearWeek> GetAllHomePlayers(int gid) {
            gameID = gid;
            var homeID = GetHomeTeamIDFromGameID(gid);
            var homePlayersID = GetAllPlayersIDOnTeam(homeID);
            var homePlayersList = GetAllStatsFromPlayersID(homePlayersID);
            homePlayersList.ForEach(player => _homePlayers.TryAdd(player.id, player));

            return _homePlayers.Values;
        }
        //https://blog.oneunicorn.com/2012/05/03/the-key-to-addorupdate/
        //Move over players to stats should keep list of home/away players
        //should also create new stats on the basis of the home/away players and store into equ statslist
        //store stats in db on create and update after game ends

        public IEnumerable<StatsYearWeek> GetAllAwayPlayers(int gid) {
            var awayID = GetAwayTeamIDFromGameID(gid);
            var awayPlayersID = GetAllPlayersIDOnTeam(awayID);
            var awayPlayersList = GetAllStatsFromPlayersID(awayPlayersID);
            awayPlayersList.ForEach(player => _awayPlayers.TryAdd(player.id, player));

            return _awayPlayers.Values;
        }

        public bool ProcessPlay(PlaysVM currPlay) {
            StatsYearWeek s = new StatsYearWeek();
            bool playStatus = true;

            foreach (PlayersVM p in currPlay.Players) {
                int SYWID = ConvertPlayerIDToSYWID(p.id);   //id conversion

                if (_playersStats.TryGetValue(SYWID, out s)) {
                    //do update on player in allLive dict
                    StatsYearWeek copy = UpdateStat(p, s, currPlay.Note);
                    //update dict value
                    _playersStats[SYWID] = copy;
                    liveUpdateList.Add(copy.id);
                }
                else { }
                //punter, defensive player, will have to figure out that later, but still need to display play
            }
            return playStatus;
        }

        private void UpdatePlayerStats(object state) {
            lock (_updatePlayersStatsLock) {
                var f = r.CurrPlay(playCount);  //go to next play
                var f2 = r2.CurrPlay(pc2);  //go to next play
                if (!gameOver) {
                    _updatingPlayerStats = ProcessPlay(f);
                    _updatingPlayerStats = ProcessPlay(f2);
                }


                if (_updatingPlayerStats) {
                    BrodcastPlay(f.Desc);
                    BrodcastPlay2(f2.Desc);
                    //go through list of sent players and update accordingly
                    //_updatingPlayerStats = true;

                    if (liveUpdateList.Count != 0) {
                        foreach (var playerid in liveUpdateList) //find id in _playersStats
                        {
                            StatsYearWeek updatedP;

                            if (_playersStats.TryGetValue(playerid, out updatedP)) {

                                if (TryUpdateHomePlayerPoint(updatedP))
                                    BroadcastPlayers(updatedP);
                                else if (TryUpdateAwayPlayerPoint(updatedP))    //should be else?
                                    BroadcastPlayers(updatedP);
                            }
                            else
                                throw new Exception("Something went wrong in updateplayerstats _playersStats.TryGetValue!");
                        }
                    }

                    _updatingPlayerStats = false;
                    liveUpdateList.Clear(); //clear for next run
                    ++playCount;
                    ++pc2;
                }
                if ((f.Qtr == 4 && f.Time == "00:00") && (f2.Qtr == 4 && f2.Time == "00:00")) {
                    _updatingPlayerStats = false;   //stop signalr//StopBrodcast();
                    BrodcastPlay("Game Over!");
                    BrodcastPlay2("Game Over!");
                    gameOver = true;
                    tempsavetodb();
                    if (currentWeekServ < 14)
                        EndWeek();
                }
            }
        }

        private void tempsavetodb() {
            using (var db = new FF()) {

                var game = db.FFGameDB.Find(gameID);
                db.Entry(game).State = System.Data.Entity.EntityState.Unchanged;
                decimal total = new decimal();
                foreach (var h in _homePlayers)
                    total += h.Value.currentPts;

                game.HScore = total;
                total = 0;
                foreach (var a in _awayPlayers)
                    total += a.Value.currentPts;

                game.VScore = total;

                foreach (var n in _playersStats)
                    db.NFLPlayerStats.Add(n.Value);

                db.SaveChanges();
            }
        }

        public void EndWeek() {
            using (var db = new FF()) {
                var game = db.FFGameDB.Find(gameID);
                FFLeague League = db.FFLeagueDB.Find(game.FFLeagueID);
                //RunLive updates score in ffgamedb

                var allGamesInCurrWeek = db.FFGameDB.Where(x => x.Week == currentWeekServ).ToList();

                foreach (FFGame g in allGamesInCurrWeek) {
                    int homeID;
                    int awayID;
                    FFTeam HomeTeam;
                    FFTeam VisTeam;
                    //pull teams if null
                    if (g.HomeTeam != null) {
                        homeID = (int)g.HomeTeamID;
                        HomeTeam = db.FFTeamDB.Find(g.HomeTeamID);
                        if (g.HScore == null) {
                            g.HScore = 0;
                        }
                    }
                    else
                        throw new Exception("FFGame Hometeam null");

                    if (g.VisTeam != null) {
                        awayID = (int)g.VisTeamID;
                        VisTeam = db.FFTeamDB.Find(g.VisTeamID);
                        if (g.VScore == null) {
                            g.VScore = 0;
                        }
                    }
                    else
                        throw new Exception("FFGame Visteam null");

                    //these shouldn't work
                    foreach (NFLPlayer p in g.HomeTeam.Players) {
                        g.HScore += p.currentPts;
                    }

                    foreach (NFLPlayer p in g.VisTeam.Players) {
                        g.VScore += p.currentPts;
                    }

                    db.Entry(HomeTeam).State = System.Data.Entity.EntityState.Unchanged;
                    db.Entry(VisTeam).State = System.Data.Entity.EntityState.Unchanged;

                    //win/loss
                    if (g.HScore > g.VScore) {
                        //HomeTeam Won
                        g.HomeTeam.Win += 1;
                        g.VisTeam.Lose += 1;
                    }
                    else if (g.VScore > g.HScore) {
                        //VisTeam Won
                        g.HomeTeam.Lose += 1;
                        g.VisTeam.Win += 1;
                    }
                    else {
                        //Tie
                        g.HomeTeam.Tie += 1;
                        g.VisTeam.Tie += 1;
                    }

                    g.HomeTeam.FPTotal += (decimal)g.HScore;
                    g.VisTeam.FPTotal += (decimal)g.VScore;
                    //delete temp proj, add currentWeek to db and update


                    db.SaveChanges();
                }
                var sett = db.Settings.Find(1);
                db.Entry(sett).State = System.Data.Entity.EntityState.Unchanged;
                sett.CurrentWeek++;
                currentWeekServ = sett.CurrentWeek;

                db.SaveChanges();
                //Check this dont think need it
                //Clients.All.OnDisconnected(true);
            }
        }

        private bool TryUpdateHomePlayerPoint(StatsYearWeek statPlayer) {
            bool found = false;
            StatsYearWeek homePlayer;

            if (_homePlayers.TryGetValue(statPlayer.id, out homePlayer)) {
                _homePlayers[statPlayer.id] = statPlayer;
                found = true;
            }

            return found;

        }

        private bool TryUpdateAwayPlayerPoint(StatsYearWeek statPlayer) {
            bool found = false;
            StatsYearWeek awayPlayer;

            if (_awayPlayers.TryGetValue(statPlayer.id, out awayPlayer)) {
                _awayPlayers[statPlayer.id] = statPlayer;
                found = true;
            }

            return found;

        }

        private void BroadcastPlayers(StatsYearWeek player) {
            Clients.All.updatePlayers(player);
        }

        private void BrodcastPlay(string play) {
            Clients.All.updatePlay(play);
        }
        private void BrodcastPlay2(string play) {
            Clients.All.updatePlay2(play);
        }
        /* Warning - Database functions ahead.  Proceed with caution.  The DB is a real bitch.
         ------------------------------------------------------------------------------*/

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
        //In-TeamID using FFTEAMNFLPLAYER Out-ints of NFLPlayersID on teamID
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
        //in-Ienum<int> PlayersID out-List<StatsYearWeek>
        public List<StatsYearWeek> GetAllStatsFromPlayersID(IEnumerable<int> PlayersID) {
            List<StatsYearWeek> PlayersOnTeamCol = new List<StatsYearWeek>();

            using (var FFContext = new FF()) {

                foreach (var playerID in PlayersID) {
                    StatsYearWeek st = new StatsYearWeek();
                    //changed to StatsYearWeek, key changed, need to update to new key
                    //old key- PlayerID, new key- Year+week+playerid

                    int SYWID = ConvertPlayerIDToSYWID(playerID);   //ID conversion

                    if (_playersStats.TryGetValue(SYWID, out st))  //from stats dict created in init
                        PlayersOnTeamCol.Add(st);
                    else
                        throw new Exception("PlayeronTeam in DB, not found pulling from statsdict created in init");
                }
            }

            return PlayersOnTeamCol;
        }
        /*
         * State functions
         */

        public void LiveStart() {
            lock (_statsStateLock) {
                if (StatsState != StatsState.Open) {
                    //_timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);
                    //MarketState = MarketState.Open;
                    //BroadcastMarketStateChange(MarketState.Open);
                }
            }
        }


        public void LiveStop() {
            lock (_statsStateLock) {
                if (StatsState == StatsState.Open) {
                    //if (_timer != null) {
                    //    _timer.Dispose();
                    //}
                    //MarketState = MarketState.Closed;
                    //BroadcastMarketStateChange(MarketState.Closed);
                }
            }
        }

        /*
         * Helper functions for NFLPlayerData 
         */

        //Takes playerID and converts it to SYWID => currentYear,currentWeekServ,playerID in int form
        public int ConvertPlayerIDToSYWID(int playerID) {
            string convertID = currentYear.ToString() + currentWeekServ.ToString() + playerID.ToString();
            int SYWID = Convert.ToInt32(convertID);

            return SYWID;
        }

        public StatsYearWeek UpdateStat(PlayersVM fromPlay, StatsYearWeek fromSYWList, string note) {
            //Take fromPlayer statID and find stat
            //Add in fumbles later, confusing, 
            //whenever a pass, 4 with no playerId comes up, 7 with no playerID -> these will get screened out because no playerID   figure out later
            //Penalties could fuck up yardage

            switch (fromPlay.StatId) {
                case 10:
                    //rush yds
                    fromSYWList.RushingStats.RushAtt += 1;
                    fromSYWList.RushingStats.RushYds += fromPlay.Yards;

                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //RushLng Don't add in right now

                    break;
                case 11:
                    //rushTDyds
                    fromSYWList.RushingStats.RushAtt += 1;
                    fromSYWList.RushingStats.RushYds += fromPlay.Yards;

                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //RushLngTD Don't add in right now
                    if (note == "TD") {
                        fromSYWList.RushingStats.RushTds += 1;
                        fromSYWList.currentPts += 6;
                    }
                    else
                        throw new Exception("Has RushingTDYards but note is not TD <- check play");
                    break;
                case 14:
                    //passInc
                    fromSYWList.PassingStats.PassAtt += 1;
                    break;
                case 15:
                    //PassYrds
                    fromSYWList.PassingStats.PassYds += fromPlay.Yards;
                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    break;
                case 16:
                    //PassYrdsTD
                    fromSYWList.PassingStats.PassYds += fromPlay.Yards;
                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));

                    if (note == "TD") {
                        fromSYWList.PassingStats.PassTds += 1;
                        fromSYWList.currentPts += 6;
                    }
                    else
                        throw new Exception("Has PassTDYards but note is not TD <- check play");
                    break;
                case 19:
                    //PassINT
                    if (note == "INT") {
                        fromSYWList.PassingStats.PassInts += 1;
                        fromSYWList.currentPts += -2;
                    }
                    //add int Exception it triggers on chargersvssteelers

                    break;
                case 20:
                    //QBSack
                    fromSYWList.PassingStats.PassAtt += 1;
                    //sack yardage goes in to team total but qb indv stats do not change

                    break;
                case 21:
                    //RecYds
                    fromSYWList.ReceivingStats.Rec += 1;
                    fromSYWList.ReceivingStats.RecYds += fromPlay.Yards;

                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    //add in RecLng Later
                    break;
                case 22:
                    //RecYrdsTD
                    fromSYWList.ReceivingStats.Rec += 1;
                    fromSYWList.ReceivingStats.RecYds += fromPlay.Yards;
                    fromSYWList.ReceivingStats.RecTds += 1;

                    fromSYWList.currentPts += Convert.ToDecimal((fromPlay.Yards / 10.00));
                    fromSYWList.currentPts += 6;
                    //add in RecLngTD later
                    break;
                case 69:
                    //FGMiss
                    if (note == "FGM") {
                        fromSYWList.KickingStats.Fga += 1;
                        fromSYWList.KickingStats.Fgyds += fromPlay.Yards;
                    }
                    else
                        throw new Exception("statID says FGM, but note doesn't <- check play");
                    break;
                case 70:
                    //FG
                    if (note == "FG") {
                        fromSYWList.KickingStats.Fga += 1;
                        fromSYWList.KickingStats.Fgm += 1;
                        fromSYWList.KickingStats.Fgyds += fromPlay.Yards;

                        fromSYWList.currentPts += Convert.ToDecimal(Math.Floor(fromPlay.Yards / 10.00));
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
                    if (note == "XP") {
                        fromSYWList.KickingStats.Xpa += 1;
                        fromSYWList.KickingStats.Xpmade += 1;
                        fromSYWList.KickingStats.Xptot += 1;

                        fromSYWList.currentPts += 1;
                    }
                    else
                        throw new Exception("statID says XP, but note doesn't <- check play");
                    break;
                case 111:
                    //yrds thrown cmp
                    fromSYWList.PassingStats.PassAtt += 1;
                    fromSYWList.PassingStats.PassCmp += 1;
                    break;
                case 112:
                    //yrds thrown inc
                    fromSYWList.PassingStats.PassAtt += 1;
                    break;
                case 113:
                    //YAC (For AdvStats)
                    break;
                case 115:
                    //PassTarget
                    fromSYWList.ReceivingStats.RecTrg += 1;
                    break;
                default:
                    //write out to file with plays that don't get flagged later after adding in fumbles, ko's, penalty, etc.
                    break;
            }

            return fromSYWList;
        }

        //set all class members to 0, default null
        public void SetSYWToZero(StatsYearWeek Stats) {

            Type m1 = Stats.PassingStats.GetType();
            Type m2 = Stats.RushingStats.GetType();
            Type m3 = Stats.ReceivingStats.GetType();
            Type m4 = Stats.KickingStats.GetType();
            Type m5 = Stats.FumbleStats.GetType();
            PropertyInfo[] myProps1 = m1.GetProperties();
            PropertyInfo[] myProps2 = m2.GetProperties();
            PropertyInfo[] myProps3 = m3.GetProperties();
            PropertyInfo[] myProps4 = m4.GetProperties();
            PropertyInfo[] myProps5 = m5.GetProperties();
            foreach (PropertyInfo p in myProps1) {
                p.SetValue(Stats.PassingStats, 0);
            }
            foreach (PropertyInfo p in myProps2) {
                p.SetValue(Stats.RushingStats, 0);
            }
            foreach (PropertyInfo p in myProps3) {
                p.SetValue(Stats.ReceivingStats, 0);
            }
            foreach (PropertyInfo p in myProps4) {
                p.SetValue(Stats.KickingStats, 0);
            }
            foreach (PropertyInfo p in myProps5) {
                p.SetValue(Stats.FumbleStats, 0);
            }
        }
    }
//}