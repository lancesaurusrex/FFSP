using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//Added
using WebApplication1.Models;

//Should be able to do for both, If the games are live and if the games are not live, isNFLLive

namespace WebApplication1.GameData {

    public class GameData {
        public int currWeek;
        public int currYear;
        public NFLWeekSetup WeekData;
        public List<string> NFLGamesID;

        public ReadJSONDatafromNFL Json_NFL;

        public GameData(int cW, int cY) {
            //Current Week/Year Data
            currWeek = cW;
            currYear = cY;
            //needs currentWeek and currentYear for NFLSeason
            WeekData = new NFLWeekSetup(currWeek, currYear);
            NFLGamesID = WeekData.CreateNFLcomIDs();
        }

        public void OfflineDataPull() {
            //Start by getting URL of data to pull from and gameID;
            //Example Address - http://www.nfl.com/liveupdate/game-center/2016091807/2016091807_gtd.json

            foreach (var id in NFLGamesID) {
                //create uri
                string NFLAddress;
                NFLAddress = "http://www.nfl.com/liveupdate/game-center/" + id + "/" + id + "_gtd.json";
                Uri NFLurl = new Uri(NFLAddress);
                Json_NFL = new ReadJSONDatafromNFL(NFLurl, id);
                Json_NFL.DeserializeData(null, id);
                //When storing in DB on DeserData, check on addorupdate stuff.  Will have problems with new NFLPlayers
                //Yea I rigged the DeserializePlayer to work for presentation.  Will need to change
            }
        }
    }
}