﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models {
    //used to be called FantasyFootball but deleted class may have to rename file?
    public class FFLeague {

        public FFLeague() { //These are the View
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
        public int NumberOfTeams { get; set; }  //Technically MAX_TEAMS
        public int NumberOfDivision { get; set; }
        public int PlayoffWeekStart { get; set; }
        public int QBStart { get; set; }
        public int RBStart { get; set; }
        public bool WRTESame { get; set; }
        public int WRStart { get; set; }
        public int TEStart { get; set; }
        public int DEFStart { get; set; }
        //Settings Containers
        public List<int> NumberOfTeamsList { get; set; }
        public List<int> NumberOfDivisionsList { get; set; }
        public List<int> NumberOfPlayoffWeekStartList { get; set; }
        public List<int> NumberOfQBStList { get; set; }
        public List<int> NumberOfRBStList { get; set; }
        public List<int> NumberOfWRStList { get; set; }
        public List<int> NumberOfTEStList { get; set; }
        public List<int> NumberOfDEFStList { get; set; }

        //Stuff in League
        //A League has multiple teams in it
        public virtual ICollection<FFTeam> Teams { get; set; }
        //A League has a schedule of many games
        public virtual ICollection<FFGame> Schedule { get; set; }
        //A League has many players/A player can be in many leagues
        //fluent API - Many to many
        public List<FFPlayer> NFLPlayerList { get; set; }
    }

    public class FFTeam {
        public FFTeam() { }
        public FFTeam(int LeagueID) {
            this.FFLeagueID = LeagueID;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FFTeamID { get; set; }
        public string TeamName { get; set; }
        public decimal Win { get; set; }
        public decimal Lose { get; set; }
        public decimal Tie { get; set; }
        public decimal FPTotal { get; set; }
        //A User "owns" a team
        public string UserID { get; set; }
        //A Team plays multiple games
        public virtual ICollection<FFGame> Games { get; set; }
        //A Team has multiple players
        public virtual ICollection<FFPlayer> Players { get; set; }
        //Team is in a league
        public virtual int FFLeagueID { get; set; }
        [ForeignKey("FFLeagueID")]
        public virtual FFLeague FFLeague { get; set; }
    }

    public class FFGame {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FFGameID { get; set; }
        public int Week { get; set; }
        public int Year { get; set; }
        public decimal HScore { get; set; }
        public decimal VScore { get; set; } 
        public int HomeTeamID { get; set; }
        public int VisTeamID { get; set; }
    }

    public class FFPlayer {

        //This is the FF(NFL)PlayerID with the link to the FFTeam, don't want to make the actual NFLPlayer class the FFPlayer class, make them seperate
        [Key]
        public int FFPlayerID { get; set; }

        //This is the seperate NFLPlayer and link to it
        //No idea if this is how I should do it, can't link to foreignkey due to different tables so fuck it we'll try this
        public NFLPlayer NFLPlayer { get; set; }

        //Weekly Scores(List of Games)

        //Ownership
        // A player has a team
        //public virtual int FFTeamID { get; set; }
        //[ForeignKey("FFTeamID")]
        //public virtual FFTeam FFTeam { get; set; }

        //A player ownership check
        public bool isOwned { get; set; }
        //A player active check
        public bool isActive { get; set; }
    }

    /*  Example keep FFPlayer just need one list of players
     *  Have a seperate list of players_team that link a playerid to a teamid, since all teamid are unique no need for leagueid!
     */
    public class Team_Player {

        public int NFLPlayerID { get; set; }
        public int Team_ID { get; set; }
        public int position { get; set; }
        //A player ownership check
        public bool isOwned { get; set; }
        //A player active check
        public bool isActive { get; set; }
    }
}

//public virtual int UserId { get; set; }

//[ForeignKey("UserId")]
//public virtual UserProfile User { get; set; }
//This way, you pair foreign key property with navigation property.

//Edit : you can also write it like this :

//[ForeignKey("UserProfile")]
//public virtual int UserId { get; set; }

//public virtual UserProfile UserProfile { get; set; }