using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models {
    public class FantasyFootball {}

    public class FFTeam {
        public int FFTeamID { get; set; }
        public string TeamName { get; set; }
        public decimal Win { get; set; }
        public decimal Lose { get; set; }
        public decimal Tie { get; set; }
        public decimal FPTotal { get; set; }
        List<string> NFLPlayerIDList { get; set; }  //List of NFLPlayer id's which are just strings.
        List<FFGame> FFGameIDList { get; set; }
    }

    public class FFGame {
        public int FFGameID { get; set; }
        public decimal HScore { get; set; }
        public decimal VScore { get; set; }
        int HomeTeamID { get; set; }
        int VisTeamID { get; set; }
    }

    public class FFLeague {      
        //settings
        public int NumberOfTeams { get; set; }
        public int PlayoffWeekStart { get; set; }
        public int QBStart { get; set; }
        public int RBStart { get; set; }
        public bool WRTESame { get; set; }
        public int WRStart { get; set; }
        public int TEStart { get; set; }
        public int DEFStart { get; set; }
        //containers
        public int FFLeagueID { get; set; }
        public List<int> FFTeamIDList { get; set; }
    }
}