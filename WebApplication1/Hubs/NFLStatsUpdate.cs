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

namespace WebApplication1.Hubs {
    public enum StatsState {
        Closed,
        Open
    }

    public class NFLStatsUpdate {
        private readonly static Lazy<NFLStatsUpdate> _instance = new Lazy<NFLStatsUpdate>(
            () => new NFLStatsUpdate(GlobalHost.ConnectionManager.GetHubContext<NFLLiveUpdateHub>().Clients));

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

        public NFLWeekSetup WeekData;       //Create NFL.com GameID's
        public List<string> NFLGamesID;     //Store for week/year in GameID's

        private NFLStatsUpdate(IHubConnectionContext<dynamic> clients) {

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

        public static NFLStatsUpdate Instance {
            get {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        
        //StatsState
        private volatile StatsState _statsState;
        private readonly object _statsStateLock = new object();

        /*
         * State functions
        */

        public StatsState StatsState {
            get { return _statsState; }
            private set { _statsState = value; }
        }

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
    }
}