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

//Interesting odds site https://fantasydata.com/nfl-stats/point-spreads-and-odds
//http://www.picksfootball.com/DrawLibrary.aspx

namespace WebApplication1.Models {
    public class FFDBInitHelper {
        private UnitOfWorkDB UOW;
        string FilePathtoSeedCSV;
        List<NFLTeam> NFLNickNamesList = new List<NFLTeam>();

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
        }

        public void AddNFLNamesListtoDB() {
            //Pull List into Add
            UOW.FFContext.NFLTeam.AddRange(NFLNickNamesList);
            //Actual Save
            UOW.FFSave();
        }

        private void AddNFLGametoDB(NFLGame ng) {
            //Add
            UOW.FFContext.NFLGame.Add(ng);
            //Actual Save
            UOW.FFSave();
        }

        public void NFLScheduleYearLooping() {

            int startYear = 2016;
            int numYears = 3;   //Do years 2016,2017 & 2018

            while (numYears > 0) {
                NFLSchedule(startYear);
                startYear += 1;
                --numYears;
            }
        }

        public void NFLSchedule(int scheduleYear) {
            //Putting in NFL Schedule 2016
            //The FileName for the schedule file must have the current year as the first 4 characters, it is how I will
            //keep track of which year is which, e.g. 20XXNFLSchedule.csv

            string fileName = scheduleYear + "NFLSchedule.csv";
            string fileRead1 = FilePathtoSeedCSV + fileName;

            //string fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\2016NFLSchedule.csv";
            //Pull Year from schedule fileName
            var actualfileName = Path.GetFileName(fileName);
            string firstfourcharactersofschedulefilename = new string(actualfileName.Take(4).ToArray());
            int currentScheduleYear = Convert.ToInt32(firstfourcharactersofschedulefilename);

            //Read NFLSchedule Game one by one and parse into fields, changing week string into int and finding NFLTeams, etc.
            
            using (StreamReader reader = new StreamReader(fileRead1)) {

                CsvReader csvReader = new CsvReader(reader);
                while (csvReader.Read()) {
                    csvReader.Configuration.WillThrowOnMissingField = false;    //CSV file has blank field for @ symbol
                    string strWeek = csvReader.GetField<string>("Week");

                    NFLGame game = new NFLGame();
                    //Check week in pull and change isNOTreg to True and change PREX to X
                    /*Will pull week column as string regardless of format, 
                     * if strWeek has Pre it is a preseason game and needs int changed to higher number
                     * then take pulled csv string and parse into int
                    */
                    if (strWeek != null) {
                        if (strWeek.Contains("Pre")) {
                            var arrWeek = strWeek.Where(Char.IsDigit).ToArray();   //remove letters and such, keep int
                            strWeek = new string(arrWeek);
                            game.IsExhibition = true;
                        }

                        int parsedString;
                        if (Int32.TryParse(strWeek, out parsedString))
                            game.Week = parsedString;
                        else
                            game.Week = null;
                    }

                    //For Format: Week,Day,CalendarDate,VisTeam,,HomeTeam,TimeEST
                    var calDate = csvReader.GetField<string>("CalendarDate");
                    var visTeam = csvReader.GetField<string>("VisTeam");
                    var homeTeam = csvReader.GetField<string>("HomeTeam");
                    var time = csvReader.GetField<string>("TimeEST");
                    var day = csvReader.GetField<string>("Day");
                    game.Day = day;
                    game.Year = currentScheduleYear;

                    /* For Format:Week,Day,CalendarDate,TimeEST,Winner/tie,at,Loser/tie,,Pts,Pts,YdsW,TOW,YdsL,TOL
                    Just need up to blank after loser, 
                    In theory, visTeam and homeTeam will be blank, rewrite code below to take both winTeam and loseTeam */
                    
                    var winTeam = csvReader.GetField<string>("Winner/tie");
                    var loseTeam = csvReader.GetField<string>("Loser/tie");
                    if (winTeam != null && loseTeam != null) {
                        var at = csvReader.GetField<string>("at");
                        //No @ means winner is hometeam
                        if (String.Compare(at,"@") == 0) { //want at == @
                            homeTeam = loseTeam;
                            visTeam = winTeam;
                        }
                        else {
                            homeTeam = winTeam;
                            visTeam = loseTeam;
                        }
                        
                    }
                    //Do date/time
                    game.DateEST = parseDateTimeString(calDate, currentScheduleYear, time);

                    //Find Visiting team in NFLTeamlist from DB
                    string nickName = null;
                    ParseCityNickname(visTeam, out nickName);
                    if (nickName != null)
                        game.VisTeamID = UOW.FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

                    //Find Home team in NFLTeamlist from DB
                    ParseCityNickname(homeTeam, out nickName);
                    if (nickName != null)
                        game.HomeTeamID = UOW.FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

                    AddNFLGametoDB(game);
                }
            }
        }


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
