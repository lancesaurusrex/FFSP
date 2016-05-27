using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Data;
using System.Text;
using WebApplication1.Models;
using WebApplication1.DAL;
using System.Data.Entity;
/// <summary>
/// Summary description for ReadJSONDatafromNFL
/// </summary>

public class ReadJSONDatafromNFL {
    public ReadJSONDatafromNFL() { }
    public ReadJSONDatafromNFL(string FileName) {
        endOfGame = false;
        isUpdate = false;
        
        count = 37;
        drivesNum = "1";

        int ID = 2015101200;                //Parse from somewhere, prob web addr call or my schedule database
        gameID = ID.ToString();      //Needs to be in string for JSON calls.

        string Root = HttpContext.Current.Server.MapPath("~/");
        string FullPath = Root + FileName;
        NFLData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(FullPath));
        totalDrives = (int)NFLData[gameID]["drives"]["crntdrv"];
        NFLPlays = new List<PlaysVM>(); 
    }
    public string gameID { get; set; }
    public bool endOfGame { get; set; }
    public bool isUpdate { get; set; }

    static public int count { get; set; }
    public JObject NFLData { get; set; }
    public int totalDrives { get; set; }
    static public string drivesNum { get; set; }
    public List<PlaysVM> NFLPlays {get;set;}

    //makeshift bs for bs sp class
    public PlaysVM NextPlay(int i) {
       return NFLPlays[i + 1];
    }

    public List<PlaysVM> QuickParseAfterLive() {
        //reset

        isUpdate = false;

        JsonSerializer serializer = new JsonSerializer();

        int playsCount = 0;

        string playNum = Convert.ToString(count);
        while (Convert.ToInt16(drivesNum) < totalDrives){

            int numplaysinDrive = (int)NFLData[gameID]["drives"][drivesNum]["numplays"];


            if (playsCount >= numplaysinDrive) {
                var a = Convert.ToInt16(drivesNum);
                ++a;
                drivesNum = a.ToString();
            }
            
            JObject playsInCurrentDrive = (JObject)NFLData[gameID]["drives"][drivesNum]["plays"];
            string posteam = (string)NFLData[gameID]["drives"][drivesNum]["posteam"];
                    //taking the key of each individual play and storing into a list
                    IList<string> playsKeys = playsInCurrentDrive.Properties().Select(p => p.Name).ToList();

                    //going through each key of the ind. play and taking out the play, players and storing into objects
                    foreach (string key in playsKeys) {
                        var play = (JObject)NFLData[gameID]["drives"][drivesNum]["plays"][key];
                        //numofplays

                        //string endplay = (string)NFLData[gameID]["drives"][drivesNum]["end"];
                        PlaysVM convPlay = (PlaysVM)serializer.Deserialize(new JTokenReader(play), typeof(PlaysVM));
                        JObject playersInCurrentPlay = (JObject)NFLData[gameID]["drives"][drivesNum]["plays"][key]["players"];
                        IList<string> playersKeys = playersInCurrentPlay.Properties().Select(p => p.Name).ToList();
                        IList<PlayersVM> PlayersList = new List<PlayersVM>();
                        //Going through each key and storing the players into players list
                        //Don't want to store player if playerkey is 0 and making sure that 
                        //the offense players are only being stored.  Play team poss is equal to the current drive team poss.
                        
                        foreach (string playerKey in playersKeys) {
                            if (playerKey != "0") { //&& possessionteam == to currentpossesionteam
                                JArray seqKeys = (JArray)NFLData[gameID]["drives"][drivesNum]["plays"][key]["players"][playerKey];

                                foreach (var seq in seqKeys) {
                                    if (posteam == (string)seq["clubcode"]) {
                                        PlayersVM pa = (PlayersVM)serializer.Deserialize(new JTokenReader(seq), typeof(PlayersVM));
                                        //statid 10 is rushing
                                        //convert to yardage and add NFLplayer to list with new yardage totals

                                        string s = RemoveSpecialCharacters(playerKey);   //converts string to int, need int for the key, need string to search JOBject
                                        int playerIDInt = Convert.ToInt32(s);
                                        pa.nflID = playerKey;
                                        pa.id = playerIDInt;

                                        PlayersList.Add(pa);
                                    }
                                }
                            }
                        }
                        convPlay.Players = PlayersList;

                        NFLPlays.Add(convPlay);

                        playsCount++;
                    }
        }
        return (NFLPlays);
    }

    //adds game to db, will not work if players are already there (week)
    public void DeserializeData(string FileName)
    {
        //string json = get_web_content("http://localhost:54551/2015101200_gtd.json"); //NFL.com address
        //dynamic game = NFLData;
        //dynamic newGame = new JObject();
        //string FileName = "2015101200_gtd.json";

        int ID = 2015101200;                //Parse from somewhere, prob web addr call or my schedule database
        string gameID = ID.ToString();      //Needs to be in string for JSON calls.

        string Root = HttpContext.Current.Server.MapPath("~/");
        string FullPath = Root + FileName;
        var results = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(FullPath));
        JObject NFLData = JObject.Parse(File.ReadAllText(FullPath));
        List<NFLEPMarkov> ExpectedPointData = new List<NFLEPMarkov>();

        string excelFileName = "MARKOVDATA.xlsx";
        string excelFullPath = Root + excelFileName;

        //How to do this better?  Don't pass the List pass a number or something to the list?  make list static?
        ExpectedPointData = LoadExpectedPointsData(excelFullPath);

        var score = (string)NFLData.SelectToken(gameID + ".home.score.1");
        //var homeScore1 = NFLData["2015101200"]["home"]["score"];
        //var homeScoreres = results[gameID]["home"]["score"];

        //var firstDrive = results[gameID]["drives"]["1"];
        //var secDrive = results[gameID]["drives"]["2"];

        string homeTeam = results[gameID]["home"]["abbr"];
        string awayTeam = results[gameID]["away"]["abbr"];
        JObject homeStats = results[gameID]["home"]["stats"];
        JObject awayStats = results[gameID]["away"]["stats"];

        DeserializePlayerStats(homeStats, homeTeam, awayStats, awayTeam);
        DeserializeTeamStats(homeStats, homeTeam, awayStats, awayTeam);

        int totalDrives = results[gameID]["drives"]["crntdrv"];     //if game is over crntdrive will be the total # of drives
        JObject drives = results[gameID]["drives"];

        DeserializeDrives(ExpectedPointData, totalDrives, drives);
    }

    public static string RemoveSpecialCharacters(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public void DeserializePlayerStats(JObject homeStats, string homeTeam, JObject awayStats, string awayTeam) {

        //This is the names of the two different properties in the JSON 
        JObject homePassing = (JObject)homeStats["passing"];
        JObject awayPassing = (JObject)awayStats["passing"];
        //Need something of all stats subcategories strings 
        //List<string> StatsChildren = new List<string>(new string[] { "passing", "rushing", "receiving", "fumbles", "kicking" });
        Dictionary<string, string> StatsChildren = new Dictionary<string, string> {
            {"passing", "PassingStats"},
            {"rushing", "RushingStats"},
            {"receiving", "ReceivingStats"},
            {"fumbles", "FumbleStats"},
            {"kicking", "KickingStats"}
         };
        //playerIDKeys.AddRange(awayPassing.Properties().Select(p => p.Name).ToList());
        Dictionary<string, JObject> JObjectHomeAway = new Dictionary<string, JObject>();
        JObjectHomeAway.Add("homeStats", homeStats);
        JObjectHomeAway.Add("awayStats", awayStats);
        List<string> playerIDStringKeys = new List<string>();
        List<NFLPlayer> PlayerList = new List<NFLPlayer>();     //List of made NFL Players

        using (var db = new FF()) {
            //goes by passing, rushing, rec, etc. goes by home/away player
            foreach (KeyValuePair<string, string> child in StatsChildren) {
                foreach (KeyValuePair<string, JObject> objName in JObjectHomeAway) {

                    JObject statsJObj = objName.Value;   //passed in from dict, is either the homeStats or awayStats jObject
                    string objPropertyName = objName.Key;
                    JObject getIDs = (JObject)statsJObj[child.Key];

                    playerIDStringKeys.AddRange(getIDs.Properties().Select(p => p.Name).ToList());
                    
                    foreach (string playerID in playerIDStringKeys) {
                        /*PsC - Create list of players
                          Get PlayerID of player about to be added
                         Compare to List of players
                         If Found*/
                        NFLPlayer NFLFoundPlayer = null;                       

                            string s = RemoveSpecialCharacters(playerID);   //converts string to int, need int for the key, need string to search JOBject
                            int playerIDInt = Convert.ToInt32(s);

                        //going through the list of already made players and pulling the player if the id's match
                        //if found add stats according to child (pass,rec, rush, etc)
                        //if not found, create player and copy material
                        if (PlayerList.Count() != 0) {
                            NFLFoundPlayer = PlayerList.Find(x => x.id == playerIDInt);
                        }


                        //Player not found if null, so create and fill in player info
                        if (NFLFoundPlayer == null) {
                            NFLFoundPlayer = new NFLPlayer();

                            NFLFoundPlayer.id = playerIDInt;    //PlayerID in int format
                            NFLFoundPlayer.id_nflformat = playerID;     //PlayerID in NFL string format
                            //Using homeStats and awayStats as property names., jObj is home or awayStats jObject                  
                            NFLFoundPlayer.name = statsJObj[child.Key][playerID]["name"].ToString();

                            if (objPropertyName == "homeStats") {
                                NFLFoundPlayer.team = homeTeam;
                            }
                            else if (objPropertyName == "awayStats") {
                                NFLFoundPlayer.team = awayTeam;
                            }
                            else {
                                NFLFoundPlayer.team = "XXX";
                            }

                        }
                        else { }

                        //Finds the correct type of stats with the playerId and puts it into a JOnject
                        var statsPullJSON = statsJObj[child.Key][playerID];

                        //takes pulled stats and adds them to the FoundPlayer
                        //if (child.Key == "passing") {
                        //    NFLFoundPlayer.PassingStats = null;//(PassingGameStats)statsPullJSON.ToObject(typeof(PassingGameStats));
                        //}
                        //else if (child.Key == "rushing") {
                        //    NFLFoundPlayer.RushingStats = null;// (RushingGameStats)statsPullJSON.ToObject(typeof(RushingGameStats));
                        //}
                        //else if (child.Key == "receiving") {
                        //    NFLFoundPlayer.ReceivingStats = null;//  (ReceivingGameStats)statsPullJSON.ToObject(typeof(ReceivingGameStats));
                        //}
                        //else if (child.Key == "fumbles") {
                        //    NFLFoundPlayer.FumbleStats = null;// (FumbleGameStats)statsPullJSON.ToObject(typeof(FumbleGameStats));
                        //}
                        //else if (child.Key == "kicking") {
                        //    NFLFoundPlayer.KickingStats = null;// (KickingGameStats)statsPullJSON.ToObject(typeof(KickingGameStats));
                        //}
                        //else { //throw exception
                        //}

                        //add in NFLPlayer to Playerlist and DB,  sep function?    
                        var dbaddorupdate = PlayerList.Find(x => x.id == NFLFoundPlayer.id);
                        if (dbaddorupdate == null)
                        {
                            PlayerList.Add(NFLFoundPlayer);
                        }
                        //checking to see if found in db if not add 
                        //Fix this sometime not sure of better way
                        var dbcheck = db.NFLPlayer.Find(NFLFoundPlayer.id);

                        if (dbcheck == null) {
                            db.NFLPlayer.Add(NFLFoundPlayer);
                            db.SaveChanges();
                        }
                        else {
                            db.Entry(NFLFoundPlayer).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        //empty keys for next iteration
                    }
                    playerIDStringKeys.Clear();
                }
            }            
        }
    }
    

    //Quick change don't need the whole table just the states, that's how I'll link them.
    public List<NFLEPMarkov> LoadExpectedPointsData(string path) {
        NFLEPMarkov MarkovTable = new NFLEPMarkov();
        List<NFLEPMarkov> ExpectedPoints = new List<NFLEPMarkov>();
        MarkovTable.GetMarkovData(path);
        foreach (var a in MarkovTable.GetMarkovData(path)) {
            ExpectedPoints.Add(a);
        }

        return ExpectedPoints;
    }

    //This function takes JObjects and finds the correct path to pull the TeamStatsData and stores in a TeamStates Object
    public void DeserializeTeamStats(JObject homeStats, string homeTeam, JObject awayStats, string awayTeam) {
        homeStats = (JObject)homeStats["team"];
        awayStats = (JObject)awayStats["team"];
        TeamStats homeStatsobj = (TeamStats)homeStats.ToObject(typeof(TeamStats));
        TeamStats awayStatsobj = (TeamStats)awayStats.ToObject(typeof(TeamStats));
    }

    public int convertToIntYardtoTD(string yardLine, string posTeam) {
        int yardNum = 0;    //is local var to hold yardLine

        string[] seperatedYardLine = yardLine.Split();
        if (seperatedYardLine == null) {
            throw new NullReferenceException("SeperatedYardLine cannot be null");
        }
        else if (seperatedYardLine[0] == "50") {    //Special Case, 50 comes in without a team name
            yardLine = "XXX 50";                        //this wont trigger if ballSideOfField == posteam
            seperatedYardLine = yardLine.Split();       //should return just 50
        }

        string ballSideOfField = seperatedYardLine[0];
        yardNum = Convert.ToInt32(seperatedYardLine[1]);

        if (yardNum < 0 || yardNum > 50) {
            throw new FormatException("ballSideOfField is null or yardNum is out of Range");
        }

        /*Trying to figure out if ball is on your side of the field or opponent's convert from teamname yrdline to yrdsfromtd
        INPUT yardLine-SD 35, posTeam-SD
        your side of the field is 51-99, opponents is 1-50
        yourSideOfTheField = true
         * output is 65, 65 yards from the endzone */
        if (ballSideOfField == posTeam) {
            yardNum = 100 - yardNum;

            if (yardNum < 50) {
                throw new FormatException("Ball is on your side of the field, yardsfromTD should be greater than 51");
            }
        }

        return yardNum;
    }

    public NFLEPMarkov getExpectedPointsForPlay(List<NFLEPMarkov> EPList, int down, int ydsToFirst, int convertedYardsToTD) {
        //need format of MarkovChains List?
        //Source Markov Drive Analysis-Google spreadsheet
        /*Find down
         * Find Yards To Go - (Format - 1,2,3,4,5,6,7,8,9,10,11,16,21)
         * Find convertedYardsToTD 
         * 
         * Code - Find down and yards to go
         * Take all remaning convertedYardsToTD and put into list and search and find closest
         * */

        //List<NFLEPMarkov> findExpectedPoints = EPList.FindAll(x => (x.Down == down));
        // && (x.YardsToGo >= ydsToFirst) && (x.YardLine < convertedYardsToTD));
        List<NFLEPMarkov> findExpectedPoints = EPList.FindAll(x => (x.Down == down) && (x.YardsToGo <= ydsToFirst));
        findExpectedPoints.Sort((a, b) => a.YardsToGo.CompareTo(b.YardsToGo));
        var temp = findExpectedPoints.Last();
        var YardsGoMarker = temp.YardsToGo;
        findExpectedPoints = EPList.FindAll(x => (x.Down == down) && (x.YardsToGo == YardsGoMarker) && (x.YardLine <= convertedYardsToTD));

        //findExpectedPoints = EPList.FindAll(x => (x.YardsToGo == ydsToFirst));
        //&& (x.YardsToGo == ydsToFirst) && (x.YardLine <= convertedYardsToTD));

        if (findExpectedPoints.Count == 0) {
            throw new NullReferenceException("findExpectedPoints should not be empty");
        }


        findExpectedPoints.Sort((a, b) => a.YardLine.CompareTo(b.YardLine));   //I don't think this is necessary anymore
        NFLEPMarkov findExpectedPointsObj = findExpectedPoints.Last();

        return findExpectedPointsObj;
    }

    public void DeserializeDrives(List<NFLEPMarkov> EPList, int totalDrives, JObject drives) {
        List<Plays> PlaysCurrDrive = new List<Plays>();
        Start sDrive = new Start();
        End eDrive = new End();
        //Goes through each drive in gameID
        for (int i = 1; i <= totalDrives; ++i) {
            //Is the current drive aka 1,2,3,etc.
            using (var db = new NFLgame()) {
                var currentDrive = (JObject)drives[i.ToString()];

                if (currentDrive != null) {


                    //throw into using, IDisposable?
                    JsonSerializer serializer = new JsonSerializer();
                    //Store currentDrive into Drives object
                    Drives storeCurrentDrive = (Drives)serializer.Deserialize(new JTokenReader(currentDrive), typeof(Drives));

                    //taking the plays out of the currentDrive and storing into JObject
                    JObject playsInCurrentDrive = (JObject)currentDrive["plays"];
                    //taking the key of each individual play and storing into a list
                    IList<string> playsKeys = playsInCurrentDrive.Properties().Select(p => p.Name).ToList();

                    //going through each key of the ind. play and taking out the play, players and storing into objects
                    foreach (string key in playsKeys) {
                        //storing the play into play object, need the unique key for each play to read
                        Plays play = (Plays)serializer.Deserialize(new JTokenReader(playsInCurrentDrive[key]), typeof(Plays));
                        if (play == null) {
                            throw new NullReferenceException("Play is not supposed to be null");
                        }

                        if (play.Down != 0) { //Means Kickoff  edit around that will have to figure out what to do with those
                            int convertedYardsToTD = convertToIntYardtoTD(play.Yrdln, play.Posteam);
                            NFLEPMarkov tempMarkov = getExpectedPointsForPlay(EPList, play.Down, play.Ydstogo, convertedYardsToTD); //Down,YardsToGo,YardLine(in 1-99 format)
                            play.EPState = tempMarkov.State;
                            play.MarkovExpPts = tempMarkov.Markov;
                        }

                        JObject playersInCurrentPlay = (JObject)currentDrive["plays"][key]["players"];

                        //Getting the key (semi-unique, it's the playerID) and storing in a list
                        IList<string> playersKeys = playersInCurrentPlay.Properties().Select(p => p.Name).ToList();
                        //Going through each key and storing the players into players list
                        //Don't want to store player if playerkey is 0 and making sure that 
                        //the offense players are only being stored.  Play team poss is equal to the current drive team poss.
                        foreach (string playerKey in playersKeys) {
                            if (playerKey != "0" && play.Posteam == storeCurrentDrive.Posteam) {
                                //storing the playerID
                                string playerID = playerKey;
                                //Putting the current play players into a list
                                IList<Players> PlayersList = serializer.Deserialize<IList<Players>>(new JTokenReader(playersInCurrentPlay[playerKey]));
                            }
                        }
                        PlaysCurrDrive.Add(play);
                    }
                    //another way to do it, didnt work as well
                    //JEnumerable<JToken> playsContainer = results[gameID]["drives"][i.ToString()]["plays"].Children();

                    //Fill in ExpPt Data for start and end drive

                    //---> should handle start and end drive data in seperate function!  <----

                    sDrive = serializer.Deserialize<Start>(new JTokenReader(currentDrive["start"]));
                    eDrive = serializer.Deserialize<End>(new JTokenReader(currentDrive["end"]));    //?  Make this live ready, you don't know the end if it's live
                    StartEndDrive(PlaysCurrDrive, sDrive, eDrive);
                    //clear PlayA List
                    Plays startPlay = PlaysCurrDrive.First();
                    sDrive.expectedPts = startPlay.MarkovExpPts;
                    Plays endPlay = PlaysCurrDrive.Last();
                    eDrive.expectedPts = endPlay.MarkovExpPts;
                    if (startPlay == null || endPlay == null) { throw new NullReferenceException("StartPlay or EndPlay is null"); }
                    AnalyzeDrivePlaysList(PlaysCurrDrive, sDrive, eDrive);  //do start and end here
                }
            }
        }
    }

    public void StartEndDrive(List<Plays> PlaysinCD, Start strDrive, End endDrive) {
        //Figure out later
    }

    public void AnalyzeDrivePlaysList(List<Plays> DrivePlays, Start StartDrive, End EndDrive) {
        List<double> expectedPointsList = new List<double>();
        List<double> epChangeList = new List<double>();

        foreach (Plays play in DrivePlays) {//get rid of 0 kickoff, can have 0 but will need to figure out how to classify kickoffs, timeouts, etc.
            if (play.MarkovExpPts != 0) {
                expectedPointsList.Add(play.MarkovExpPts);
            }
        }

        for (int i = 0; i < expectedPointsList.Count; ++i) {
            if (i != 0) {
                //vauleNow - ValueLast
                double epChange = (expectedPointsList[i]) - (expectedPointsList[i - 1]);
                epChangeList.Add(epChange);
            }
        }
    }


    public string get_web_content(string url) {
        Uri uri = new Uri(url);
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
        request.Method = WebRequestMethods.Http.Get;
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string output = reader.ReadToEnd();
        response.Close();

        return output;
    }
}