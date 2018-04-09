namespace WebApplication1.Migrations {
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using WebApplication1.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication1.DAL.FF> {
        public Configuration() {
            AutomaticMigrationsEnabled = false;
            ContextKey = "WebApplication1.DAL.FF";
            //Debug seed if necessary
            if (System.Diagnostics.Debugger.IsAttached == false) {
                //System.Diagnostics.Debugger.Launch();
                //System.Diagnostics.Debugger.Break();  //Force Break at this point
            }
        }

        protected override void Seed(WebApplication1.DAL.FF FFContext) {
//Add NFLTeams and NFLSchedule to DB.

            FFDBInitHelper a = new FFDBInitHelper();

            a.NFLNicknames1();
            a.NFLTeams2();
            a.NFLAbbr3();
            a.AddNFLNamesListtoDB();
            a.NFLSchedule();



            /*This method will be called after migrating to the latest version.

            You can use the DbSet<T>.AddOrUpdate() helper extension method 
            to avoid creating duplicate seed data. E.g.
            
              context.People.AddOrUpdate(
                p => p.FullName,
                new Person { FullName = "Andrew Peters" },
                new Person { FullName = "Brice Lambson" },
                new Person { FullName = "Rowan Miller" }
              );
            

          League Testing
          Due to autoinc on id, this adds a new league with a different id then */
            FFLeague ffl1 = new FFLeague();
            ffl1.FFLeagueID = 2;
            ffl1.FFLeagueName = "Lardo";

            FFContext.FFLeagueDB.Add(ffl1);
            FFContext.SaveChanges();
            //League Testing, addorupdate with same id as created
            FFContext.FFLeagueDB.AddOrUpdate(p => p.FFLeagueName, new FFLeague { FFLeagueName = "Lardo3", NumberOfTeams = 8, PlayoffWeekStart = 14 });
            FFContext.SaveChanges();
            //screwy code ahead that works
            //seeding FFteams with while loop

            FFLeague leag = FFContext.FFLeagueDB.Where(y => y.FFLeagueName == "Lardo3").FirstOrDefault();   //finds league by name

            if (leag != null) {
                int j = 0;
                while (j != 8) {
                    FFContext.FFTeamDB.AddOrUpdate( //adds or updates dbentry by teamName
                        p => p.TeamName, new FFTeam
                        {
                            TeamName = "Team" + j++,
                            FFLeagueID = leag.FFLeagueID,   //found league id
                            UserID = "737a0a07-a158-40de-b6c2-131e36e22038"
                        }
                        );
                    FFContext.SaveChanges();
                }


                //Add Teams to League List
                var TeamsInLeague = (from t in FFContext.FFTeamDB where t.FFLeagueID == leag.FFLeagueID select t).ToList();
                foreach (FFTeam t in TeamsInLeague)
                    leag.Teams.Add(t);
            }
            else
                throw new NullReferenceException("League doesnt exist on Context call LeagueName == Lardo3");


            FFContext.FFLeagueDB.AddOrUpdate(
                p => p.FFLeagueID, new FFLeague { FFLeagueID = 1 }

                );

<<<<<<< HEAD
=======
            //NFLNicknames FIle
            /* Take NFL Nicknames from CSV file and addorupdate NFLTeam DB update-*/

            string fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\NFLNicknames.csv";

            //Putting in NFLNicknames
            using (StreamReader reader = new StreamReader(fileName)) {

                CsvReader csvReader = new CsvReader(reader);
                while (csvReader.Read()) {
                    csvReader.Configuration.WillThrowOnMissingField = true;

                    string nicknameField = csvReader.GetField<string>("Nickname");
                    NFLTeam team = new NFLTeam(nicknameField);
                    FFContext.NFLTeam.AddOrUpdate(t => t.Nickname, team);
                }
            }

            //NFLTeams FIle
            fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\NFLTeams.csv";
            //Pulling Nickname from NFLTeam DB and adding city, Putting in FullTeamName (City + NickName)

            using (StreamReader reader = new StreamReader(fileName)) {

                CsvReader csvReader = new CsvReader(reader);
                csvReader.Configuration.WillThrowOnMissingField = true;
                while (csvReader.Read()) {
                    var fullName = csvReader.GetField<string>("City");
                    //parse nickname from field, count spaces if more than 1 take word after 2nd space, 1 take next word
                    string nickName;
                    var city = ParseCityNickname(fullName, out nickName);
                    var team = FFContext.NFLTeam.Local.Single(c => c.Nickname == nickName);
                    team.City = city;
                    FFContext.NFLTeam.AddOrUpdate(c => c.Nickname, team);
                }
            }

            //NFLABBRV
            fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\NFLAbbrv.csv";
            //Pulling Nickname from NFLTeam DB and adding abbr
            using (StreamReader reader = new StreamReader(fileName)) {

                CsvReader csvReader = new CsvReader(reader);
                csvReader.Configuration.WillThrowOnMissingField = true;
                while (csvReader.Read()) {
                    var nickName = csvReader.GetField<string>("Nickname");
                    var abbr = csvReader.GetField<string>("Abbr");
                    //parse nickname from field, count spaces if more than 1 take word after 2nd space, 1 take next word
                    var team = FFContext.NFLTeam.Local.Single(c => c.Nickname == nickName);
                    team.Abbr = abbr;
                    FFContext.NFLTeam.AddOrUpdate(c => c.Nickname, team);
                }
                FFContext.SaveChanges();
            }

            /* Muy Importante - 
             * Current week: http://www.nfl.com/liveupdate/scorestrip/ss.xml
             * http://www.nfl.com/ajax/scorestrip?season=%d&seasonType=%s&week=%d
             * seasonType can be PRE, REG, or POST. For reg or post, weeks go 1-22 (18 is WC, 19 Div, 20 Conf Championship, 21 is blank, 22 is SB). For pre, 0-4 IIRC.
             */


            //Putting in NFL Schedule 2016
            fileName = "C:\\Users\\Lance\\Source\\Repos\\FFSP\\WebApplication1\\SeedCSV\\2016NFLSchedule.csv";
            //Read NFLSchedule Game one by one and parse into fields, changing week string into int and finding NFLTeams, etc.

            using (StreamReader reader = new StreamReader(fileName)) {

                CsvReader csvReader = new CsvReader(reader);
                while (csvReader.Read()) {
                    csvReader.Configuration.WillThrowOnMissingField = false;
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
                            game.NOTRegular = true;
                        }

                        int parsedString;
                        if (Int32.TryParse(strWeek, out parsedString))
                            game.Week = parsedString;
                        else
                            game.Week = null;
                    }

                    //Week,Day,CalendarDate,VisTeam,,HomeTeam,TimeEST
                    var calDate = csvReader.GetField<string>("CalendarDate");
                    var visTeam = csvReader.GetField<string>("VisTeam");
                    var homeTeam = csvReader.GetField<string>("HomeTeam");
                    var time = csvReader.GetField<string>("TimeEST");
                    var day = csvReader.GetField<string>("Day");
                    game.Day = day;
                    game.Year = currentYear;

                    //Do date/time
                    game.DateEST = parseDateTimeString(calDate, currentYear, time);

                    //Find Visiting team in NFLTeamlist from DB
                    string nickName = null;
                    ParseCityNickname(visTeam, out nickName);
                    if (nickName != null)
                        game.VisTeamID = FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

                    //Find Home team in NFLTeamlist from DB
                    ParseCityNickname(homeTeam, out nickName);
                    if (nickName != null)
                        game.HomeTeamID = FFContext.NFLTeam.Where(t => t.Nickname == nickName).Select(t => t.Abbr).Single();

                    FFContext.NFLGame.Add(game);
                    FFContext.SaveChanges();
                }
            }
        }

        /*CUSTOM NFL PARSE FUNCTIONS, Parse(NFL)CityNickName, ParseDateTime(calendarDate,year,team)*/
        //parses city from full NFLTeam name, will only parse if there is one or two spaces in fullName
        //e.g. Arizona Cardinals->Arizona, New England Patriots->New England, San Tuskaloo Red Storm -> null
        string ParseCityNickname(string fullTeamName, out string nickName) {
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

        //FunTimes!!!11
        DateTime parseDateTimeString(string calDate, int year, string time) {

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

>>>>>>> 4ab2a1ea13af06969e5a651a14227b5449012e8a
        }
    }
}
