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

namespace WebApplication1.Controllers
{
    public class FFTeamsController : Controller
    {
        private FF db = new FF();


        //Add Team to League
        public ActionResult CreateTeam(int LeagueID) {
            
            //Passed LeagueID through HTML.Hidden from (CreateTeam) get to post.  
            ViewBag.LeagueID = LeagueID;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTeam([Bind(Include = "FFTeamID,TeamName,Win,Lose,Tie,FPTotal,FFLeagueID")] FFTeam FFTeam) {
            if (ModelState.IsValid) {

                db.FFTeamDB.Add(FFTeam);               
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(FFTeam);
        }

        // GET: FFTeams
        public ActionResult Index()
        {
            var fFTeamDB = db.FFTeamDB.Include(f => f.FFLeague);
            return View(fFTeamDB.ToList());
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
