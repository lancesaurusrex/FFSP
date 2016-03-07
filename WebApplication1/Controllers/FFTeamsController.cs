using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAL;
using WebApplication1.Models;
using Microsoft.AspNet.Identity;

//Merge Two Collections without duplicates, elegant and speedy
//var dict = AllPlayers.ToDictionary(x => x.id);
//foreach (var player in AllPlayersTaken) {
//    dict[player.id] = player;
//}
//var mergedPlayers = dict.Values.ToList();

namespace WebApplication1.Controllers {
    public class FFTeamsController : Controller {
        private FF db = new FF();
        const int CURRENTWEEK = 1;

        //Displays All AvailablePlayers and can add selected ones to the team
        public ActionResult AvailablePlayers(int TeamID) {
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague = db.FFLeagueDB.Find(FFTeam.FFLeagueID);

            var allPlayersIDTeamsInLeague = GetAllPlayersIDOnTeamsInLeague(GetAllTeamIDFromLeague(FFTeam.FFLeagueID));

            ViewBag.TeamID = TeamID;
            Session["TeamID"] = TeamID;
            return View(GetAllPlayersOnTeamsInLeague(allPlayersIDTeamsInLeague));
        }

        //I dont think passing AvailablePlayer is necessary
        [HttpPost]
        public ActionResult AvailablePlayers(IList<NFLPlayer> AvailablePlayer, FormCollection collection) {
            //Pass TeamID from get to post
            var PassedTID = Session["TeamID"];
            int TeamID = Convert.ToInt32(PassedTID);
            Session["TeamID"] = null;
            //Find team and league in db
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague1 = db.FFLeagueDB.Find(FFTeam.FFLeagueID);

            foreach (string id in collection.Keys) {
                //if the key is a digit in the collection and FFTeam is found
                if (id.All(Char.IsDigit) && FFTeam != null && FFLeague1 != null) {
                    //Find NFLPlayer
                    int playerIDconvert = Convert.ToInt32(id);
                    NFLPlayer NFLPlayer = db.NFLPlayer.Find(playerIDconvert);
                    //Add to Team;s Players list
                    if (NFLPlayer != null)
                        FFTeam.Players.Add(NFLPlayer);  //add player to team                   
                    else
                        throw new NoNullAllowedException("Player not found in NFLPlayerDB");
                    //add player teamplayer db
                    //could make a addplayer function
                    TeamNFLPlayer TeamPlayer = new TeamNFLPlayer();
                    TeamPlayer.TNPID = Convert.ToInt32("" + TeamID + playerIDconvert);
                    TeamPlayer.PlayerID = playerIDconvert;
                    TeamPlayer.TeamID = TeamID;
                    TeamPlayer.isActive = false;

                    db.FFTeamNFLPlayer.Add(TeamPlayer);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index", new { TeamID = FFTeam.FFTeamID });
        }

        public IEnumerable<FFGame> FillWeekScoreboard(int TeamID, int? numWeek) {
            FFTeam team = db.FFTeamDB.Find(TeamID);

            var leagueSchedule = team.FFLeague.Schedule;

            if (leagueSchedule != null || leagueSchedule.Count != 0) {

            }
            else { }
                //Pull schedule from db

            IEnumerable<FFGame> currentWeekSchedule = null;

            if (numWeek == null)
                currentWeekSchedule = leagueSchedule.Where(x => x.Week == CURRENTWEEK);
            else
                currentWeekSchedule = leagueSchedule.Where(x => x.Week == numWeek);

            return currentWeekSchedule;

        }

        public ActionResult Scoreboard(int TeamID) {

            //FillWeekScoreboard(int TeamID, int? numWeek)    
            var currentWeek = FillWeekScoreboard(TeamID, null);
            //signalr asp.net tutorial

            return View();  //compiler stop bitching
        }

        /* Database Pulls
         * GetAllFFPlayers - Return all NFL Players from the NFL.com pull site
         * GetAllTeamIDFromLeague - Return all TeamIDs in one League
         * GetAllPlayersIDOnTeamsInLeague - Return all PlayerID's from all teams in one league
         * GetAllPlayersOnTeamsInLeagues - Return all Players from all teams in one league
         --------------------------------------------------------------------------------------------------*/
        //Gets all NFLPlayer from NFLDB
        public IQueryable<NFLPlayer> GetAllFFPlayers() {
            return db.NFLPlayer;
        }

        //Gets All Teams from a LeagueID **Returns collection of teamID's
        public ICollection<int> GetAllTeamIDFromLeague(int leagueID) {
            var TeamsInLeague = (from t in db.FFTeamDB where t.FFLeagueID == leagueID select t.FFTeamID).ToList();
            return TeamsInLeague;
        }

        //Gets All Player ID on All Teams in one league  
        public IQueryable<int> GetAllPlayersIDOnTeamsInLeague(ICollection<int> AllTeamIDInLeague) {

            IQueryable<int> AllPlayersTakenID = null;

            foreach (int TeamID in AllTeamIDInLeague) {
                AllPlayersTakenID = (from p in db.FFTeamNFLPlayer where p.TeamID == TeamID select p.PlayerID);
            }

            return AllPlayersTakenID;
        }

        //Gets all players on all teams in one league into NFLPlayer obj 
        public ICollection<NFLPlayer> GetAllPlayersOnTeamsInLeague(IQueryable<int> AllPlayersTakenID) {
            List<NFLPlayer> AllPlayersTaken = new List<NFLPlayer>();
            List<NFLPlayer> AllPlayers = GetAllFFPlayers().ToList();

            //finding each takenplayer by id in AllPlayers and storing them as a player
            foreach (int playerID in AllPlayersTakenID) {

                AllPlayersTaken.Add(AllPlayers.Find(x => x.id == playerID));
            }

            //Looping through allplayerslist and removing players that are taken
            //AllPlayers = AllPlayers.Intersect(AllPlayersTaken).ToList();
            var dict = AllPlayers.ToDictionary(x => x.id);
            foreach (var player in AllPlayersTaken) {
                dict.Remove(player.id);
            }
            var mergedPlayers = dict.Values.ToList();

            return mergedPlayers;
        }
        //-----------------------------------------------------------------------------------------------------

        public ActionResult RemovePlayers(int TeamID) {

            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague = db.FFLeagueDB.Find(FFTeam.FFLeagueID);

            var PlayersOnTeam = FFTeam.Players.ToList();

            ViewBag.TeamID = TeamID;
            Session["TeamID"] = TeamID;

            return View(PlayersOnTeam);
        }

        [HttpPost]
        public ActionResult RemovePlayers(FormCollection Collection) {
            //Pass TeamID from get to post
            var PassedTID = Session["TeamID"];
            int TeamID = Convert.ToInt32(PassedTID);
            Session["TeamID"] = null;

            //Find team and league in db
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague1 = db.FFLeagueDB.Find(FFTeam.FFLeagueID);

            foreach (string id in Collection.Keys) {

                if (id.All(Char.IsDigit) && FFTeam != null && FFLeague1 != null) {
                    int playerIDconvert = Convert.ToInt32(id);
                    NFLPlayer NFLPlayer = db.NFLPlayer.Find(playerIDconvert);
                    if (NFLPlayer != null)
                        //Remove From FFTeam Players List
                        FFTeam.Players.Remove(NFLPlayer);
                    else
                        throw new NullReferenceException("NFLPlayer not found in NFLPlayer db, has to exist to remove");
                    //Remove from TeamPlayers DB
                    //Could put remove in seperate function
                    int TNPID = Convert.ToInt32("" + TeamID + playerIDconvert);
                    TeamNFLPlayer TeamPlayer = db.FFTeamNFLPlayer.Find(TNPID);

                    if (TeamPlayer != null) {
                        db.FFTeamNFLPlayer.Remove(TeamPlayer);
                        db.SaveChanges();
                    }
                    else
                        throw new NullReferenceException("TeamPlayer should not be null, it has to be in the FFTeamNFLPlayerdb to be deleted.");
                }
            }

            return RedirectToAction("Index", new { TeamID = FFTeam.FFTeamID });
        }

        public ActionResult ViewTeamsInLeague(int TeamID) {
            FFTeam Team = db.FFTeamDB.Find(TeamID);
            FFLeague League = db.FFLeagueDB.Find(Team.FFLeagueID);

            if (League != null)
                return View(League.Teams);
            else
                throw new NullReferenceException("League cannot be null to view all teams in league");
        }

        public ActionResult ViewPlayersOnTeam(int TeamID) {

            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);

            if (FFTeam.Players != null)
                return View(FFTeam.Players);
            else
                throw new NoNullAllowedException("No Players on Team");
        }
        /*
        Schedule Time Ladies and Gents!
        2 ways to access schedule League.Schedule and Team.FFLeague.Games
        */
        //Make Link in Schedule?
        public ActionResult Schedule(int TeamID) {
            FFTeam Team = db.FFTeamDB.Find(TeamID);
            FFLeague League = db.FFLeagueDB.Find(Team.FFLeagueID);
            ViewBag.TeamID = TeamID;

            if (League.Schedule == null)
                throw new NullReferenceException("League.Schedule cannot be null");
            else
                return View(Team.FFLeague.Schedule);
        }

        public ActionResult CreateSchedule(int TeamID) {
            FFTeam Team = db.FFTeamDB.Find(TeamID);
            FFLeague League = db.FFLeagueDB.Find(Team.FFLeagueID);

            //Make filled into numofteams
            List<int> Listof0oppo = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
            List<int> Listof1oppo = new List<int> { 0, 2, 3, 4, 5, 6, 7 };
            List<int> Listof2oppo = new List<int> { 0, 1, 3, 4, 5, 6, 7 };
            List<int> Listof3oppo = new List<int> { 0, 1, 2, 4, 5, 6, 7 };
            List<int> Listof4oppo = new List<int> { 0, 1, 2, 3, 5, 6, 7 };
            List<int> Listof5oppo = new List<int> { 0, 1, 2, 3, 4, 6, 7 };
            List<int> Listof6oppo = new List<int> { 0, 1, 2, 3, 4, 5, 7 };
            List<int> Listof7oppo = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

            if ( (League.Schedule.Count() == 0) && (League.NumberOfTeams == League.Teams.Count()) )
            {
                if (Team.DivisionID == 0 || Team.DivisionID == null) {
                    Dictionary<int, List<int>> DictOfOppoList = new Dictionary<int, List<int>>();
                    Dictionary<int, List<int>> MasterDict = new Dictionary<int, List<int>>();
                    DictOfOppoList.Add(0, Listof0oppo);
                    DictOfOppoList.Add(1, Listof1oppo);
                    DictOfOppoList.Add(2, Listof2oppo);
                    DictOfOppoList.Add(3, Listof3oppo);
                    DictOfOppoList.Add(4, Listof4oppo);
                    DictOfOppoList.Add(5, Listof5oppo);
                    DictOfOppoList.Add(6, Listof6oppo);
                    DictOfOppoList.Add(7, Listof7oppo);
                    MasterDict = DictOfOppoList.ToDictionary(entry => entry.Key, entry => entry.Value);

                    for (int weekCntr = 1; weekCntr < League.PlayoffWeekStart; ++weekCntr) {

                        if (weekCntr == League.NumberOfTeams) { //Reset when opponents run out
                            Listof0oppo = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
                            Listof1oppo = new List<int> { 0, 2, 3, 4, 5, 6, 7 };
                            Listof2oppo = new List<int> { 0, 1, 3, 4, 5, 6, 7 };
                            Listof3oppo = new List<int> { 0, 1, 2, 4, 5, 6, 7 };
                            Listof4oppo = new List<int> { 0, 1, 2, 3, 5, 6, 7 };
                            Listof5oppo = new List<int> { 0, 1, 2, 3, 4, 6, 7 };
                            Listof6oppo = new List<int> { 0, 1, 2, 3, 4, 5, 7 };
                            Listof7oppo = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
                            MasterDict.Clear();
                            MasterDict.Add(0, Listof0oppo);
                            MasterDict.Add(1, Listof1oppo);
                            MasterDict.Add(2, Listof2oppo);
                            MasterDict.Add(3, Listof3oppo);
                            MasterDict.Add(4, Listof4oppo);
                            MasterDict.Add(5, Listof5oppo);
                            MasterDict.Add(6, Listof6oppo);
                            MasterDict.Add(7, Listof7oppo);
                        }
                        //Reset for next schedule Week
                        List<FFTeam> NoChangeListTeams = League.Teams.ToList();
                        List<int> WeekCheckList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                        //Refill Running Delete List with current values
                        DictOfOppoList = MasterDict.ToDictionary(entry => entry.Key, entry => entry.Value);

                        while (DictOfOppoList.Count != 0 )  //Only have enough opponents for x(NUMOFTEAM) amount of weeks
                        {
                            var WeekCheckListIterator = 0;  //needs to reset on every iteration

                            var oppoList = DictOfOppoList.First().Value;  //oppoList is ListofXopponent
                            int FirstTeamKey = DictOfOppoList.First().Key;  //The Key is the num of the first team

                            int oppIndex = -1;   //jump into while for first result, -1 = nf so do..while, will inf.loop if never found

                            while (oppIndex == -1) {  //if not found keep going
                                //Find the Index of the first opponent in the oppoX List, if not found go to next available opponent in weekcheck(opponent)list
                                oppIndex = oppoList.FindIndex(x => x == WeekCheckList[WeekCheckListIterator]);
                                WeekCheckListIterator += 1;
                            }

                            var secondTeamoppoListValue = oppoList[oppIndex];   //Storing value (opponentNum) in var

                            var firstTeam = NoChangeListTeams.ElementAt(FirstTeamKey);
                            var secondTeam = NoChangeListTeams.ElementAt(secondTeamoppoListValue);

                            //FFGame(int pWeek, int pYear, int pHomeTeamID, int pVisTeamID)
                            FFGame Game = new FFGame(weekCntr, FFLeague.YEAR, firstTeam.FFTeamID, secondTeam.FFTeamID);
                            League.Schedule.Add(Game);

                            db.FFGameDB.Add(Game);
                            db.SaveChanges();

                            //Remove opponents from oppolist in dictionary
                            DictOfOppoList[FirstTeamKey].Remove(secondTeamoppoListValue);
                            DictOfOppoList[secondTeamoppoListValue].Remove(FirstTeamKey);
                            //Need MasterDict to keep the values of the Lists, I have to delete the entries from the main list
                            //Copy removed value list to MasterDict
                            MasterDict[FirstTeamKey] = DictOfOppoList[FirstTeamKey];
                            MasterDict[secondTeamoppoListValue] = DictOfOppoList[secondTeamoppoListValue];

                            DictOfOppoList.Remove(FirstTeamKey);  //Remove FirstTeam from Dict
                            var removeDict = DictOfOppoList.First(x => x.Key == (secondTeamoppoListValue));
                            DictOfOppoList.Remove(removeDict.Key);

                            WeekCheckList.Remove(FirstTeamKey);//remove boths values from weekcheecklist
                            WeekCheckList.Remove(secondTeamoppoListValue);
                        }
                    }
                }
            }
            else {
                string err = "Need correct amount of Teams in League to create schedule or League Schedule has games in it already";
                return View(err);
            }

            return RedirectToAction("Schedule", new { TeamID = Team.FFTeamID });
        }

        //Add Team to League
        [Authorize]
        public ActionResult CreateTeam(int LeagueID) {

            //Passed LeagueID through HTML.Hidden from (CreateTeam) get to post.  
            ViewBag.LeagueID = LeagueID;

            return View();
        }

        //Needs to be signed in! for get and post
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTeam([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam FFTeam) {
            if (ModelState.IsValid) {

                //User Needs to be logged in (Authorize)
                FFTeam.UserID = User.Identity.GetUserId();
                //Find League in DB
                FFLeague CurrLeague = db.FFLeagueDB.Find(FFTeam.FFLeagueID);
                //Add team to model/db
                CurrLeague.Teams.Add(FFTeam);
                db.FFTeamDB.Add(FFTeam);
                db.SaveChanges();
                return RedirectToAction("Index", new { TeamID = FFTeam.FFTeamID });
            }

            return View(FFTeam);
        }

        //code snippet displays teams in league var fFTeamDB = db.FFTeamDB.Include(f => f.FFLeague);
        // GET: FFTeams
        public ActionResult Index(int TeamID) {
            //load once will have to figure out how to do that
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague League = db.FFLeagueDB.Find(FFTeam.FFLeagueID);
            //Team View, Edit Team, Team Schedule
            return View(FFTeam);
        }

        public ActionResult StartLive() {
            NFLLiveViewModel n = new NFLLiveViewModel();
            n.dostuff();

            return View();
        }
        // GET: FFTeams/Details/5
        public ActionResult Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null) {
                return HttpNotFound();
            }
            return View(fFTeam);
        }

        // GET: FFTeams/Create
        public ActionResult Create() {
            //ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName");
            return View();
        }

        // POST: FFTeams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam fFTeam) {
            if (ModelState.IsValid) {

                db.FFTeamDB.Add(fFTeam);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName", fFTeam.FFLeagueID);
            return View(fFTeam);
        }

        // GET: FFTeams/Edit/5
        public ActionResult Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null) {
                return HttpNotFound();
            }
            ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName", fFTeam.FFLeagueID);
            return View(fFTeam);
        }

        // POST: FFTeams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam fFTeam) {
            if (ModelState.IsValid) {
                db.Entry(fFTeam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName", fFTeam.FFLeagueID);
            return View(fFTeam);
        }

        // GET: FFTeams/Delete/5
        public ActionResult Delete(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null) {
                return HttpNotFound();
            }
            return View(fFTeam);
        }

        // POST: FFTeams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            db.FFTeamDB.Remove(fFTeam);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public void runLive(int gameID) {

            
        }

    }
}



//Schedule Algortihtntngkm
/*Best idea - Make this code simple as possible atm.  Take first and second of list and have them play each other
Listof0oppo = 1,2,3,4,5,6,7
Listof1opoo = 0,2,3,4,5,6,7
Listof2oppo = 0,1,3,4,5,6,7
...
Weekoppo = 0,1,2,3,4,5,6,7
each List take first num
Listof0 is 1 -> 0v1, rem 0,1 from weekoppo, remove individual opponent from list
WeekOppo = 2,3,4,5,6,7 
List of Lists -> next list 2
Listof2 is 0,++,1,++,3,2v3, rem 2,3
WeekOppo = 4,5,6,7
LoL -> 4
Listof4 0,++,5 rem 4,5
LoL->6
Listof6 0,++,7 rem 6,7
next iter */