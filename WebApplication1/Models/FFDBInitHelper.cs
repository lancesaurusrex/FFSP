using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvHelper;
using System.IO;
using System.Globalization;
using WebApplication1.Models;
using System.Data.Entity;
using WebApplication1.DAL;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

//Interesting odds site https://fantasydata.com/nfl-stats/point-spreads-and-odds
//http://www.picksfootball.com/DrawLibrary.aspx

namespace WebApplication1.Models {
    public class FFDBInitHelper {
        private UnitOfWorkDB UOW;
        string FilePathtoSeedCSV;
        private List<NFLTeam> NFLNickNamesList = new List<NFLTeam>();

        //http://mehdi.me/ambient-dbcontext-in-ef6/
        //https://www.codeguru.com/csharp/.net/net_asp/mvc/using-the-repository-pattern-with-asp.net-mvc-and-entity-framework.htm

        public FFDBInitHelper() {
            UOW = new UnitOfWorkDB();
            
            //setting up relative path to csv files.  Filepath changes from cpu to cpu
            string pathtoFileName = "\\SeedCSV\\";

            string dirName = AppDomain.CurrentDomain.BaseDirectory; // Starting Dir
            FileInfo fileInfo = new FileInfo(dirName);
            DirectoryInfo parentDir = fileInfo.Directory.Parent;
            string parentDirName = parentDir.FullName; // Parent of Starting Dir

            FilePathtoSeedCSV = parentDirName + pathtoFileName;
        }

        /*  The way the csv files were made by mystery people.  The Nickname1 func/file stores just NFL nicknames in a list.
         *  The NFL Teams file is comma-sep by full city and nickname (e.g. Atlanta Falcons, Chicago Funzos, ...)
         *  NFLTeams2 func uses a parse function to seperate city&nickname,  It then uses the nicknames from the first function 
         *  to find the nicknames and match the city to it.
         *  The NFL Abbrv csv file is abbr,nn (e.g. NE, Patriots, CHI, Funzos)  Takes Nickname and matches in list and adds abbrv
         */

        public void NFLNicknames1() {
            //NFLNicknames FIle
            /* Take NFL Nicknames from CSV file and addorupdate NFLTeam DB update-*/

            string fileName = "NFLNicknames.csv";
            string fileRead1 = FilePathtoSeedCSV + fileName;

            //Putting in NFLNicknames
            using (StreamReader reader = new StreamReader(fileRead1)) {

                CsvReader csvReader = new CsvReader(reader);
                while (csvReader.Read()) {
                    csvReader.Configuration.WillThrowOnMissingField = true;

                    string nicknameField = csvReader.GetField<string>("Nickname");
                    NFLTeam team = new NFLTeam(nicknameField);
                    NFLNickNamesList.Add(team);
                }
            }


            //AddOrUpdate(t => t.Nickname, team);
        }

        public void NFLTeams2() {
            //NFLTeams FIle
            string fileName = "NFLTeams.csv";
            string fileRead1 = FilePathtoSeedCSV + fileName;

            //string fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\NFLTeams.csv";
            //Pulling Nickname from NFLTeam DB and adding city, Putting in FullTeamName (City + NickName)

            using (StreamReader reader = new StreamReader(fileRead1)) {

                CsvReader csvReader = new CsvReader(reader);
                csvReader.Configuration.WillThrowOnMissingField = true;
                while (csvReader.Read()) {
                    var fullName = csvReader.GetField<string>("City");
                    //parse nickname from field, count spaces if more than 1 take word after 2nd space, 1 take next word
                    string nickName;
                    var city = ParseCityNickname(fullName, out nickName);
                    var team = NFLNickNamesList.Find(c => c.Nickname == nickName);

                    team.City = city;   //Does this update the obj in list?
                }
            }
        }

        public void NFLAbbr3() {

            string fileName = "NFLAbbrv.csv";
            string fileRead1 = FilePathtoSeedCSV + fileName;
            //NFLABBRV
            //string fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\NFLAbbrv.csv";
            //Pulling Nickname from NFLTeam DB and adding abbr
            using (StreamReader reader = new StreamReader(fileRead1)) {

                CsvReader csvReader = new CsvReader(reader);
                csvReader.Configuration.WillThrowOnMissingField = true;
                while (csvReader.Read()) {
                    var nickName = csvReader.GetField<string>("Nickname");
                    var abbr = csvReader.GetField<string>("Abbr");
                    //parse nickname from field, count spaces if more than 1 take word after 2nd space, 1 take next word
                    var team = NFLNickNamesList.Find(c => c.Nickname == nickName);
                    team.Abbr = abbr;    //Does this update the list entry?
                }
            }
            //Done with pulling from 3 nflteam related csv files, NFLX, abbrv, niucnames, teams

            UOW.AddRangeNFLTeamToDB(NFLNickNamesList);

            UOW.FFSave();
        }

        public string StrTypeConvert(int a) {

                if (a == 0)
                    return "PRE";
                else if (a == 1)
                    return "REG";
                else if (a == 2)
                    return "POST";
                else
                    throw new Exception();
            }
        enum seasonType {
            PRE,REG,POST
        }

        public int NFLGameCount() {
            return UOW.FFContext.NFLGame.Count();
        }
        public void PullSchedulesFromNFLcom(int scheduleYear, int week, int seasonType) {
            
            XMLHelper t = new XMLHelper();
            string strType = StrTypeConvert(seasonType);
            //whats are loop constraints?  PRE to POST
           
                string url = "www.nfl.com/ajax/scorestrip?season=" + scheduleYear + "&seasonType=" + strType + "&week=" + week.ToString();
                ss xmlData = new ss();
                xmlData = t.GetXmlRequest<ss>(url);
                ssGms[] xmlArr = null;
                if (xmlData.Items != null)
                    xmlArr = xmlData.Items.ToArray();
                else
                    xmlArr = new ssGms[0];
                foreach (var i in xmlArr) {
                    var gameArr = i.g.ToArray();
                    //Check Game database for games already in DO TIHIS B4 RUNN
                    //run count of week and compare at end, throw ex if not equal
                    foreach (var game in gameArr) {
                        NFLGame gameObj = new NFLGame();
                        //set week & year of game
                        gameObj.Week = Convert.ToInt32(i.w);
                        gameObj.Year = Convert.ToInt32(i.y);
                        //set date and time of game
                        gameObj.StartTime = game.t;
                        gameObj.Day = game.d;
                        //set id
                        gameObj.Id = Convert.ToInt32(game.eid);
                        gameObj.GSIS = Convert.ToInt32(game.gsis);

                        if (game.q == "F" || game.q == "FO") {
                            gameObj.HScore = Convert.ToInt32(game.hs);
                            gameObj.VScore = Convert.ToInt32(game.vs);
                        }

                        //FIGURED OUT A PROBLEM what happen when a team like the STL RAMS is added?  Crash and burn
                        //Find NFLTeams and add ID to game
                        try {
                            NFLTeam home = UOW.FFContext.NFLTeam.Find(game.h);
                            NFLTeam away = UOW.FFContext.NFLTeam.Find(game.v);
                            List<NFLTeam> excpList = new List<NFLTeam>();
                            if (home == null) {
                                NFLTeam team = new NFLTeam(game.hnn, game.hnn, game.h);
                                excpList.Add(team);
                            }
                            if (away == null) {
                                NFLTeam team = new NFLTeam(game.vnn, game.vnn, game.v);
                                excpList.Add(team);
                            }
                            if (excpList.Count() > 0) {
                                var ex = new KeyNotFoundException() { Data = { { "excpList", excpList } } };
                                throw ex;
                            }

                            if (home != null)
                                gameObj.HomeTeamID = home.Abbr;
                            if (away != null)
                                gameObj.VisTeamID = away.Abbr;
                        }
                        catch (KeyNotFoundException ex) {
                            UOW = new UnitOfWorkDB();
                            Debug.WriteLine(ex);

                            foreach (System.Collections.DictionaryEntry de in ex.Data) {
 
                                NFLTeam NT = null;
                                foreach (var item in (dynamic)(de.Value)) {
                                    NT = (NFLTeam)item;
                                }
                                UOW.AddNFLTeamToDB(NT);
                            }

                            UOW.FFSave();
                        }
                        //add in gameType enum?
                        if (seasonType == 0)
                            gameObj.IsExhibition = true;

                        UOW.AddNFLGameToDB(gameObj);
                    }
                    
                }

        UOW.FFSave();
        }

        //Can prob delete this
        //public void NFLScheduleYearLooping() {

        //    int startYear = 2016;
        //    int numYears = 3;   //Do years 2016,2017 & 2018

        //    //Can do two different NFLSchedule looping through the years with the one file and using startYear 0 for 
        //    //the other NFLlines file
        //    while (numYears > 0) {
        //        NFLSchedule(startYear);
        //        startYear += 1;
        //        --numYears;
        //    }

        //    //Now do for new NFLlinesfile
        //    NFLSchedule(0);
        //}

        //public void NFLSchedule(int currentScheduleYear) {


        //    string filePath = FileNameWithCurrentYear(currentScheduleYear);
        //    int year = currentScheduleYear;
        //    //Read NFLSchedule Game one by one and parse into fields, changing week string into int and finding NFLTeams, etc.
            
        //    using (StreamReader reader = new StreamReader(filePath)) {
                
        //        CsvReader csvReader = new CsvReader(reader);
        //        while (csvReader.Read()) {
        //            csvReader.Configuration.WillThrowOnMissingField = false;    //CSV file has blank field for @ symbol
        //            string strWeek = csvReader.GetField<string>("Week");

        //            //check on file, using different file on 0
        //            if (currentScheduleYear == 0) {
        //                year = csvReader.GetField<int>("Year");
        //            }

        //            NFLGame game = new NFLGame();
        //            //Check week in pull and change isNOTreg to True and change PREX to X
        //            /*Will pull week column as string regardless of format, 
        //             * if strWeek has Pre it is a preseason game and needs int changed to higher number
        //             * then take pulled csv string and parse into int
        //            */
        //            if (strWeek != null) {
        //                if (strWeek.Contains("Pre")) {
        //                    var arrWeek = strWeek.Where(Char.IsDigit).ToArray();   //remove letters and such, keep int
        //                    strWeek = new string(arrWeek);
        //                    game.IsExhibition = true;
        //                }

        //                int parsedString;
        //                if (Int32.TryParse(strWeek, out parsedString))
        //                    game.Week = parsedString;
        //                else
        //                    game.Week = null;
        //            }

        //            //For Format: Week,Day,CalendarDate,VisTeam,,HomeTeam,TimeEST
        //            var calDate = csvReader.GetField<string>("CalendarDate");
        //            var visTeam = csvReader.GetField<string>("VisTeam");
        //            var homeTeam = csvReader.GetField<string>("HomeTeam");
        //            var time = csvReader.GetField<string>("TimeEST");
        //            var day = csvReader.GetField<string>("Day");
        //            game.Day = day;
        //            game.Year = year;

        //            /* For Format:Week,Day,CalendarDate,TimeEST,Winner/tie,at,Loser/tie,,Pts,Pts,YdsW,TOW,YdsL,TOL
        //            Just need up to blank after loser, 
        //            In theory, visTeam and homeTeam will be blank, rewrite code below to take both winTeam and loseTeam */
                    
        //            var winTeam = csvReader.GetField<string>("Winner/tie");
        //            var loseTeam = csvReader.GetField<string>("Loser/tie");
        //            if (winTeam != null && loseTeam != null) {
        //                var at = csvReader.GetField<string>("at");
        //                //No @ means winner is hometeam
        //                if (String.Compare(at,"@") == 0) { //want at == @
        //                    homeTeam = loseTeam;
        //                    visTeam = winTeam;
        //                }
        //                else {
        //                    homeTeam = winTeam;
        //                    visTeam = loseTeam;
        //                }
                        
        //            }
        //            //***Will need to figure out best way to parse multiple formats from both filetypes
        //            game.DateEST = parseDateTimeString(calDate, year, time);

        //            //Find Visiting team in NFLTeamlist from DB
        //            string nickName = null;
        //            ParseCityNickname(visTeam, out nickName);
        //            if (nickName != null)
        //                game.VisTeamID = UOW.FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

        //            //Find Home team in NFLTeamlist from DB
        //            ParseCityNickname(homeTeam, out nickName);
        //            if (nickName != null)
        //                game.HomeTeamID = UOW.FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

        //            AddNFLGametoDB(game);
        //        }
        //    }
        //}

        //public string FileNameWithCurrentYear(int currentScheduleYear) {
        //    string fileName;
        //    //Do lines file (somewhat different using currentScheduleYear equal to zero)
        //    if (currentScheduleYear == 0)
        //        fileName = "1995currentLines.csv";
        //    else {
        //        //Putting in NFL Schedule 2016
        //        //The FileName for the schedule file must have the current year as the first 4 characters, it is how I will
        //        //keep track of which year is which, e.g. 20XXNFLSchedule.csv
        //        fileName = currentScheduleYear + "NFLSchedule.csv";                
        //    }
        //    //string fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\2016NFLSchedule.csv";

        //    string fileRead1 = FilePathtoSeedCSV + fileName;

        //    return fileRead1;
        //}

        public string ParseCityNickname(string fullTeamName, out string nickName) {

            /*CUSTOM NFL PARSE FUNCTIONS, Parse(NFL)CityNickName, ParseDateTime(calendarDate,year,team)*/
            //parses city from full NFLTeam name, will only parse if there is one or two spaces in fullName
            //e.g. Arizona Cardinals->Arizona, New England Patriots->New England, San Tuskaloo Red Storm -> null

            string city = null;
            nickName = null;
            fullTeamName = fullTeamName.Trim();

            if (fullTeamName != null || fullTeamName != string.Empty) {

                int countSpaces = fullTeamName.Count(Char.IsWhiteSpace);

                //Arizona Cardinals
                if (countSpaces == 1) {
                    int firstSpace = fullTeamName.IndexOf(" ");
                    city = fullTeamName.Substring(0, firstSpace);
                    nickName = fullTeamName.Substring(firstSpace + 1, (((fullTeamName.Length) - (firstSpace + 1))));
                }
                else if (countSpaces == 2) {    //New England Patriots
                    int lastSpace = fullTeamName.LastIndexOf(" ");
                    city = fullTeamName.Substring(0, lastSpace);
                    nickName = fullTeamName.Substring(lastSpace + 1, ((fullTeamName.Length) - (lastSpace + 1)));
                }
            }
            return city;
        }

        public DateTime parseDateTimeString(string calDate, int year, string time) {


            //FunTimes!!!11


            string[] splitcalday = calDate.Split();
            CultureInfo enUS = new CultureInfo("en-us");
            DateTime dt;

            if (splitcalday[0] != null && splitcalday[1] != null) {
                int numMonth = DateTime.ParseExact(splitcalday[0], "MMMM", CultureInfo.InvariantCulture).Month;

                string dts = numMonth.ToString() + "/" + splitcalday[1].ToString() + "/" + year.ToString() + " " + time;

                if (DateTime.TryParseExact(dts, "g", enUS, DateTimeStyles.None, out dt)) { }
                else { DateTime.TryParse(dts, out dt); }
            }
            else
                throw new ArgumentNullException("Spliting calDate from 2016schedule didn't work");

            return dt;

        }
    }
}
