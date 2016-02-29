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


namespace WebApplication1.Controllers
{
    public class FFTeamsController : Controller
    {
        private FF db = new FF();
        private NFLdatabase NFLdb = new NFLdatabase();

        public IQueryable<NFLPlayer> GetAllFFPlayers()
        {
            //if call in another function will crash due to still accessing context
            using (var NFLContext = new NFLdatabase()) {

                return NFLContext.NFLPlayer;
            }
        }

        //should use viewmodel due to only needing 3 of the data things, formcollection?
        public ActionResult AvailablePlayers(int TeamID)
        {
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague = db.FFLeagueDB.Find(FFTeam.FFLeagueID);
            IList<NFLPlayer> AvailablePlayer = FFLeague.NFLPlayerList.FindAll(x=>x.isAvailable==true);
            ViewBag.TeamID = TeamID;
            Session["TeamID"] = TeamID;
            return View(AvailablePlayer);
        }

        [HttpPost]
        public ActionResult AvailablePlayers(IList<NFLPlayer> AvailablePlayer, FormCollection collection) {
            //Pass TeamID from get to post
            var TeamID = Session["TeamID"];
            Session["TeamID"] = null;
            //Find team and league in db
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            FFLeague FFLeague1 = db.FFLeagueDB.Find(FFTeam.FFLeagueID);

            foreach (string id in collection.Keys) {

                if (id.All(Char.IsDigit) && FFTeam != null) {

                    NFLPlayer NFLPlayer = NFLdb.NFLPlayer.Find(id);
                    FFTeam.Players.Add(NFLPlayer);  //add player to team
                    //add player teamplayer db

                    //flip isAvailable from leagueplayerlist
                    NFLPlayer LeaguePlayerFound = FFLeague1.NFLPlayerList.Find(x => x.id == Convert.ToInt32(id));
                    if (LeaguePlayerFound != null)
                        LeaguePlayerFound.isAvailable = false;
                    else
                        throw new NoNullAllowedException("Player not Found in League!");
                }
            }

            return View(AvailablePlayer);
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
        public ActionResult Index(int TeamID)
        {
            //load once will have to figure out how to do that
            FFTeam FFTeam = db.FFTeamDB.Find(TeamID);
            //Team View, Edit Team, Team Schedule
            return View(FFTeam);
        }

        // GET: FFTeams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null)
            {
                return HttpNotFound();
            }
            return View(fFTeam);
        }

        // GET: FFTeams/Create
        public ActionResult Create()
        {
            //ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName");
            return View();
        }

        // POST: FFTeams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam fFTeam)
        {
            if (ModelState.IsValid)
            {
                
                db.FFTeamDB.Add(fFTeam);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName", fFTeam.FFLeagueID);
            return View(fFTeam);
        }

        // GET: FFTeams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null)
            {
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
        public ActionResult Edit([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam fFTeam)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fFTeam).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FFLeagueID = new SelectList(db.FFLeagueDB, "FFLeagueID", "FFLeagueName", fFTeam.FFLeagueID);
            return View(fFTeam);
        }

        // GET: FFTeams/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            if (fFTeam == null)
            {
                return HttpNotFound();
            }
            return View(fFTeam);
        }

        // POST: FFTeams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FFTeam fFTeam = db.FFTeamDB.Find(id);
            db.FFTeamDB.Remove(fFTeam);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
