using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models {
//used to be called FantasyFootball but deleted class may have to rename file?
    public class FFLeague {

        public FFLeague() {
            NumberOfTeamsList = new List<int>() { 8, 10, 12 };
            NumberOfDivisionsList = new List<int>() { 2, 3, 4 };
            NumberOfPlayoffWeekStartList = new List<int>() { 14, 15 };
            //I think I don't need to make seperate lists for these (repeating), but for customization/simplification if code changes, I'll keep them this way.
            NumberOfQBStList = new List<int>() { 0, 1, 2 };
            NumberOfRBStList = new List<int>() { 0, 1, 2, 3 };
            NumberOfWRStList = new List<int>() { 0, 1, 2, 3 };
            NumberOfTEStList = new List<int>() { 0, 1, 2 };
            NumberOfDEFStList = new List<int>() { 0, 1, 2 };
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FFLeagueID { get; set; }
        public string FFLeagueName { get; set; }
        //settings
        public int NumberOfTeams { get; set; }
        public int NumberOfDivision { get; set; }
        public int PlayoffWeekStart { get; set; }
        public int QBStart { get; set; }
        public int RBStart { get; set; }
        public bool WRTESame { get; set; }
        public int WRStart { get; set; }
        public int TEStart { get; set; }
        public int DEFStart { get; set; }
        //containers       
        public List<int> FFTeamIDList { get; set; }
        //Settings Containers
        public List<int> NumberOfTeamsList { get; set; }
        public List<int> NumberOfDivisionsList { get; set; }
        public List<int> NumberOfPlayoffWeekStartList { get; set; }
        public List<int> NumberOfQBStList { get; set; }
        public List<int> NumberOfRBStList { get; set; }
        public List<int> NumberOfWRStList { get; set; }
        public List<int> NumberOfTEStList { get; set; }
        public List<int> NumberOfDEFStList { get; set; }
        //A League has multiple teams in it
        public virtual ICollection<FFTeam> Teams { get; set; }
    }

    public class FFTeam {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FFTeamID { get; set; }
        public string TeamName { get; set; }
        public decimal Win { get; set; }
        public decimal Lose { get; set; }
        public decimal Tie { get; set; }
        public decimal FPTotal { get; set; }
        List<int> NFLPlayerIDList { get; set; }  //List of NFLPlayer id's which are just strings.
        List<FFGame> FFGameIDList { get; set; }
        //A Team plays multiple games
        public virtual ICollection<FFGame> Games { get; set; }
        //A Team has multiple players
        public virtual ICollection<NFLPlayer> Players { get; set; }
    }

    public class FFGame {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FFGameID { get; set; }
        public decimal HScore { get; set; }
        public decimal VScore { get; set; }
        int HomeTeamID { get; set; }
        int VisTeamID { get; set; }
    }

    //public class FFLeagueTeams {
    //    [Key]
    //    [ForeignKey("FFLeagueID")]
    //    public virtual FFLeague League { get; set; }
    //    [ForeignKey("FFTeamID")]
    //    public virtual FFTeam Team { get; set; }
    //    public string UserID { get; set; }
    //    public bool isCommish { get; set; }
    //}

    //public class FFTeamPlayers {

    //    [Key]
    //    [ForeignKey("FFTeam")]
    //    public virtual FFTeam Team { get; set; }
    //    [ForeignKey("NFLPlayer")]
    //    public virtual NFLPlayer NFLPlayer { get; set; }
    //}
}

//public virtual int UserId { get; set; }

//[ForeignKey("UserId")]
//public virtual UserProfile User { get; set; }
//This way, you pair foreign key property with navigation property.

//Edit : you can also write it like this :

//[ForeignKey("UserProfile")]
//public virtual int UserId { get; set; }

//public virtual UserProfile UserProfile { get; set; }