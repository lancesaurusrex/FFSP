using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;


/// <summary>
/// Summary description for NFLPlayer
/// </summary>
public class NFLPlayer {
    public NFLPlayer()
    {
        isAvailable = true; 
    }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int id { get; set; }

    //actual format on nfl.com, keeping just in case needed at some point
    public string id_nflformat { set; get; }

    public string name { get; set; }
    public string team { get; set; }
    public string pos { get; set; }
    public decimal currentPts { get; set; }
    //might of got rid of this
    public bool isAvailable { get; set; }
    //For Availability in TeamController
    public bool isChecked { get; set; }
}
[Serializable]
public class NFLTeam
{
    public NFLTeam() { IsDefunct = false; }
    public NFLTeam(string nn) { City = null; Nickname = nn; TeamPlayers = null; NFLSchedule = null; }
    public NFLTeam(string city, string nn, string abbr) { Abbr = abbr; City = city; Nickname = nn; currentLocation = " "; IsDefunct = true; TeamPlayers = null; NFLSchedule = null; }
    //A team has players, A player can only be on one team
    //A team has games, plays other teams on a weekly basis
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Abbr { get; set; }
    public string City { get; set; }
    public string Nickname { get; set; }
    //For teams no longer around, IsDefunct will say if they are still around and add currentLocation
    public bool IsDefunct { get; set; }
    public string currentLocation { get; set; }

    public virtual ICollection<NFLPlayer> TeamPlayers { get; set; }
    public virtual ICollection<NFLGame> NFLSchedule { get; set; }
    
}

public class NFLGame {
    //A game has players, players can play in one game per week but can be in multiple games
    //A game has 2 teams, all game goes in a week
    //A game has stats for the 2teams
    public NFLGame() { IsExhibition = false; }
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int GSIS { get; set; }   //This come from NFL.com/ajax/scorestrip, it may or may not be necessary
    public DateTime? DateEST { get; set; }
    public string StartTime { get; set; }
    public string Day { get; set; }
    public int? Week { get; set; }  //bad parse set to null
    public int Year { get; set; }
    //A game has one NFL Hteam
    public string HomeTeamID { get; set; }
    //A game has one NFL Vteam
    public string VisTeamID { get; set; }
    public int? HScore { get; set; }
    public int? VScore { get; set; }
    public bool IsExhibition { get; set; }
}

public class StatsYearWeek {

    public StatsYearWeek() {         
        PassingStats = new PassingGameStats();
        RushingStats = new RushingGameStats();
        ReceivingStats = new ReceivingGameStats();
        FumbleStats = new FumbleGameStats();
        KickingStats = new KickingGameStats();
        //placeholders for live
        name = null;
        team = null;
        pos = null;
    }
    //For when live kicks in, this will make it WAY easier, don't need it in db though
    //Fill in model RUNLive and check if the other way works foreach on model, if not run the notmapped placeholders 
    [NotMapped]
    public string name { get; set; }
    [NotMapped]
    public string team { get; set; }
    [NotMapped]
    public string pos { get; set; }
    //Composite Primary Key with PlayerID, Year, Week, would be [Key, Column(Order = X)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int id { get; set; }
    public int PlayerID { get; set; }
    //A Stat belongs to/has one NFLPlayer
    [ForeignKey("PlayerID")]
    public virtual NFLPlayer StatsNFLPlayer { get; set; }
    public int Year { get; set; }
    public int Week { get; set; }
    public decimal currentPts { get; set; }

    public PassingGameStats PassingStats { get; set; }
    public RushingGameStats RushingStats { get; set; }
    public ReceivingGameStats ReceivingStats { get; set; }
    public FumbleGameStats FumbleStats { get; set; }
    public KickingGameStats KickingStats { get; set; }
}

public class PassingGameStats
{
    //Should add in targets
    [JsonProperty("att")]
    public int? PassAtt { get; set; }

    [JsonProperty("cmp")]
    public int? PassCmp { get; set; }

    [JsonProperty("yds")]
    public int? PassYds { get; set; }

    [JsonProperty("tds")]
    public int? PassTds { get; set; }

    [JsonProperty("ints")]
    public int? PassInts { get; set; }

    [JsonProperty("twopta")]
    public int? PassTwopta { get; set; }

    [JsonProperty("twoptm")]
    public int? PassTwoptm { get; set; }
}

public class RushingGameStats
{

    [JsonProperty("att")]
    public int? RushAtt { get; set; }

    [JsonProperty("yds")]
    public int? RushYds { get; set; }

    [JsonProperty("tds")]
    public int? RushTds { get; set; }

    [JsonProperty("lng")]
    public int? RushLng { get; set; }

    [JsonProperty("lngtd")]
    public int? RushLngtd { get; set; }

    [JsonProperty("twopta")]
    public int? RushTwopta { get; set; }

    [JsonProperty("twoptm")]
    public int? RushTwoptm { get; set; }
}

public class ReceivingGameStats
{

    [JsonProperty("rec")]
    public int? Rec { get; set; }

    [JsonProperty("yds")]
    public int? RecYds { get; set; }

    [JsonProperty("tds")]
    public int? RecTds { get; set; }

    [JsonProperty("lng")]
    public int? RecLng { get; set; }

    [JsonProperty("lngtd")]
    public int? RecLngtd { get; set; }

    [JsonProperty("twopta")]
    public int? RecTwopta { get; set; }

    [JsonProperty("twoptm")]
    public int? RecTwoptm { get; set; }

    [JsonIgnoreAttribute]
    public int? RecTrg { get; set; }
}

public class FumbleGameStats
{

    [JsonProperty("tot")]
    public int? Tot { get; set; }

    [JsonProperty("rcv")]
    public int? Rcv { get; set; }

    [JsonProperty("trcv")]
    public int? Trcv { get; set; }

    [JsonProperty("yds")]
    public int? Yds { get; set; }

    [JsonProperty("lost")]
    public int? Lost { get; set; }
}

public class KickingGameStats
{
    [JsonProperty("fgm")]
    public int? Fgm { get; set; }

    [JsonProperty("fga")]
    public int? Fga { get; set; }

    [JsonProperty("fgyds")]
    public int? Fgyds { get; set; }

    [JsonProperty("totpfg")]
    public int? Totpfg { get; set; }

    [JsonProperty("xpmade")]
    public int? Xpmade { get; set; }

    [JsonProperty("xpmissed")]
    public int? Xpmissed { get; set; }

    [JsonProperty("xpa")]
    public int? Xpa { get; set; }

    [JsonProperty("xpb")]
    public int? Xpb { get; set; }

    [JsonProperty("xptot")]
    public int? Xptot { get; set; }
}

public class TeamStats
{
    [JsonProperty("totfd")]
    public int Totfd { get; set; }

    [JsonProperty("totyds")]
    public int Totyds { get; set; }

    [JsonProperty("pyds")]
    public int Pyds { get; set; }

    [JsonProperty("ryds")]
    public int Ryds { get; set; }

    [JsonProperty("pen")]
    public int Pen { get; set; }

    [JsonProperty("penyds")]
    public int Penyds { get; set; }

    [JsonProperty("trnovr")]
    public int Trnovr { get; set; }

    [JsonProperty("pt")]
    public int Pt { get; set; }

    [JsonProperty("ptyds")]
    public int Ptyds { get; set; }

    [JsonProperty("ptavg")]
    public int Ptavg { get; set; }

    [JsonProperty("top")]
    public string Top { get; set; }
}

public class GSettings {
    [Key]
    public int id { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CurrentWeek { get;set;}
}