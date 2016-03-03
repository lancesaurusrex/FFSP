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

//https://datatellblog.wordpress.com/2015/02/11/unit-of-work-repository-entity-framework-and-persistence-ignorance/

namespace WebApplication1.Controllers {
    public class FFLeagueController : Controller {
        private FF db = new FF();

        //GET:FFLEAGUE/JOINLEAGUE
        public ActionResult JoinLeague() {

            return View();
        }

        //POST FFLEAGUE/JoinLeague
        /*  JoinLeague is by League
         * 
         *  Enter LeagueID to join that league. 
            JoinLeague searches db by leagueID, then checks to see if the league is full or not */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinLeague([Bind(Include = "FFLeagueID")]FFLeague FFLeagueCO) {

            FFLeague FFLeagueFound = db.FFLeagueDB.Find(FFLeagueCO.FFLeagueID);

            if (FFLeagueFound != null) {
                //Count numofTeams in League
                var sql = "SELECT COUNT(FFLeagueID) FROM FFTeams WHERE FFLeagueID = " + FFLeagueFound.FFLeagueID;
                var numTeams = db.Database.SqlQuery<int>(sql).Single();
                //NumTeams = num of teams currently in league, FFLea... = MAX_TEAMS allowed in league

                if (numTeams < FFLeagueFound.NumberOfTeams)
                    return RedirectToAction("CreateTeam", "FFTeams", new { LeagueID = FFLeagueFound.FFLeagueID });
                else
                    return View("Error - League Full");
            }
            else
                return View();

        }

        //Tells 

        //league was created and sends LeagueID to CreateTeam 
        //Create League Created Successfully screen
        public ActionResult CreateLeagueSuccess(int id, string leagueName) {
            //When League is first created make LeagueTeams object and insert creator into db
            //Creator of league is automatically commish
            ViewBag.leagueName = leagueName;
            ViewBag.id = id;

            return View();
        }



        public ActionResult LeagueXMainPage() {
            //if (User != null) {


            //}
            //var currentUser = User.Identity.Name;
            //string userId = ((Guid)Membership.GetUser().ProviderUserKey).ToString();
            //var ident = System.Web.HttpContext.Current.User.Identity;
            //what do we want to do with the main page without enough teams
            //if numberofregistered teams == numofteams
            return View();
        }

        //Gets All LeagueId and TeamId From UserID: returns Dictionary int, int <TeamID, LeagueID>
        public IDictionary<int, int> GetLeagueIDsFromUserID(string UserID) {

            var s = db.FFTeamDB.Where(x => x.UserID == UserID).ToDictionary(x => x.FFTeamID, x => x.FFLeagueID);
            return s;
        }

        //Gets League object from a LeagueID: returns FFLEague
        FFLeague GetLeagueFromLeagueID(int LeagueID) {

            FFLeague league = db.FFLeagueDB.Find(LeagueID);
            return league;
        }

        // GET: FFLeague
        public ActionResult Index() {

            string UserID = User.Identity.GetUserId();
            if (UserID != null) {
                var ListOfLeagueIDFromUserID = GetLeagueIDsFromUserID(UserID);
                var TeamIDLeaguesFromUserID = new Dictionary<int,FFLeague>();

                if (ListOfLeagueIDFromUserID != null) {

                    foreach (KeyValuePair<int,int> TeamAndLeagueIDs in ListOfLeagueIDFromUserID) {

                        TeamIDLeaguesFromUserID.Add(TeamAndLeagueIDs.Key, GetLeagueFromLeagueID(TeamAndLeagueIDs.Value));

                    }
                    return View(TeamIDLeaguesFromUserID);
                }
                else { throw new Exception("Join a league"); }
            }
            else
                return View(db.FFLeagueDB.ToList());    //won't work redirect to CreateLeagues
        }

        // GET: FFLeague/Details/5
        public ActionResult Details(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null) {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // GET: FFLeague/Create
        public ActionResult Create() {
            FFLeague FFLeagueCO = new FFLeague();

            return View(FFLeagueCO);
            //return View(new FFLeague());
        }

        // POST: FFLeague/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FFLeagueID, FFLeagueName,NumberOfTeams,NumberOfDivision,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague FFLeagueCO) {
            if (ModelState.IsValid) {
                //Add Players to League
                db.FFLeagueDB.Add(FFLeagueCO);
                db.SaveChanges();
                return RedirectToAction("CreateLeagueSuccess", new { id = FFLeagueCO.FFLeagueID, leagueName = FFLeagueCO.FFLeagueName });   //Goto Create League Success then create team
            }

            return View(FFLeagueCO);
        }

        // GET: FFLeague/Edit/5
        public ActionResult Edit(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null) {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // POST: FFLeague/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FFLeagueID,FFLeagueName,NumberOfTeams,NumberOfDivision,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague FFLeagueCO) {
            if (ModelState.IsValid) {
                db.Entry(FFLeagueCO).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(FFLeagueCO);
        }

        // GET: FFLeague/Delete/5
        public ActionResult Delete(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null) {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // POST: FFLeague/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            db.FFLeagueDB.Remove(FFLeagueCO);
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
