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

                if (id.All(Char.IsDigit) && FFTeam != null) {
                    int playerIDconvert = Convert.ToInt32(id);
                    NFLPlayer NFLPlayer = db.NFLPlayer.Find(playerIDconvert);

                    if (NFLPlayer != null)
                        FFTeam.Players.Add(NFLPlayer);  //add player to team                   
                    else
                        throw new NoNullAllowedException("Player not found in NFLPlayerDB");
                    //add player teamplayer db

                    //db.Entry(FFTeam).State = EntityState.Modified;
                    //db.SaveChanges();
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
        public ICollection<NFLPlayer> GetAllPlayersOnTeamsInLeague(IQueryable<int> AllPlayersTakenID){
            List<NFLPlayer> AllPlayersTaken = new List<NFLPlayer>();
            List<NFLPlayer> AllPlayers = GetAllFFPlayers().ToList();

            //finding each takenplayer by id in AllPlayers and storing them as a player
            foreach (int playerID in AllPlayersTakenID ) {

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

        public ActionResult ViewPlayersOnTeam(int TeamID) {

            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);

            if (FFTeam.Players != null)
                return View(FFTeam.Players);
            else
                throw new NoNullAllowedException("No Players on Team");

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
    }
}
