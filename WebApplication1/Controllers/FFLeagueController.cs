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

//https://datatellblog.wordpress.com/2015/02/11/unit-of-work-repository-entity-framework-and-persistence-ignorance/

namespace WebApplication1.Controllers
{
    public class FFLeagueController : Controller
    {
        private FF db = new FF();

        //GET:FFLEAGUE/JOINLEAGUE
        public ActionResult JoinLeague() 
        {
            //FFLeague FFLeagueJoin = new FFLeague();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinLeague([Bind(Include = "FFLeagueID")]FFLeague FFLeagueJoin) {

            FFLeague FFLeagueFound = db.FFLeagueDB.Find(FFLeagueJoin.FFLeagueID);

            if (FFLeagueFound != null)
                return RedirectToAction("CreateTeam", "FFTeams", new { LeagueID = FFLeagueFound.FFLeagueID });
            else
                return RedirectToAction("Index");
        }

        //Tells user league was created and sends LeagueID to CreateTeam 
        //Create League Created Successfully screen
        public ActionResult CreateLeagueSuccess(int id, string leagueName)
        {
            //When League is first created make LeagueTeams object and insert creator into db
            //Creator of league is automatically commish
            ViewBag.leagueName = leagueName;
            ViewBag.id = id;

            return View();
        }



        public ActionResult LeagueXMainPage()
        {
            //var currentUser = User.Identity.Name;
            //string userId = ((Guid)Membership.GetUser().ProviderUserKey).ToString();
            //var ident = System.Web.HttpContext.Current.User.Identity;
            //what do we want to do with the main page without enough teams
            //if numberofregistered teams == numofteams
            return View();
        }

        // GET: FFLeague
        public ActionResult Index()
        {
           return View(db.FFLeagueDB.ToList());
        }

        // GET: FFLeague/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null)
            {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // GET: FFLeague/Create
        public ActionResult Create()
        {
            return View(new FFLeague());
        }

        // POST: FFLeague/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FFLeagueID, FFLeagueName,NumberOfTeams,NumberOfDivision,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague FFLeagueCO)
        {
            if (ModelState.IsValid)
            {
                db.FFLeagueDB.Add(FFLeagueCO);
                db.SaveChanges();
                return RedirectToAction("CreateLeagueSuccess", new { id = FFLeagueCO.FFLeagueID, leagueName = FFLeagueCO.FFLeagueName });   //Goto Create League Success then create team
            }

            return View(FFLeagueCO);
        }

        // GET: FFLeague/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null)
            {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // POST: FFLeague/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FFLeagueID,FFLeagueName,NumberOfTeams,NumberOfDivision,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague FFLeagueCO)
        {
            if (ModelState.IsValid)
            {
                db.Entry(FFLeagueCO).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(FFLeagueCO);
        }

        // GET: FFLeague/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            if (FFLeagueCO == null)
            {
                return HttpNotFound();
            }
            return View(FFLeagueCO);
        }

        // POST: FFLeague/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FFLeague FFLeagueCO = db.FFLeagueDB.Find(id);
            db.FFLeagueDB.Remove(FFLeagueCO);
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
