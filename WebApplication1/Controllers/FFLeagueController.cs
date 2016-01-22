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
            FFLeague fFLeague = db.FFLeagueDB.Find(id);
            if (fFLeague == null)
            {
                return HttpNotFound();
            }
            return View(fFLeague);
        }

        // GET: FFLeague/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FFLeague/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FFLeagueID,NumberOfTeams,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague fFLeague)
        {
            if (ModelState.IsValid)
            {
                db.FFLeagueDB.Add(fFLeague);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fFLeague);
        }

        // GET: FFLeague/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague fFLeague = db.FFLeagueDB.Find(id);
            if (fFLeague == null)
            {
                return HttpNotFound();
            }
            return View(fFLeague);
        }

        // POST: FFLeague/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FFLeagueID,NumberOfTeams,PlayoffWeekStart,QBStart,RBStart,WRTESame,WRStart,TEStart,DEFStart")] FFLeague fFLeague)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fFLeague).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fFLeague);
        }

        // GET: FFLeague/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FFLeague fFLeague = db.FFLeagueDB.Find(id);
            if (fFLeague == null)
            {
                return HttpNotFound();
            }
            return View(fFLeague);
        }

        // POST: FFLeague/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FFLeague fFLeague = db.FFLeagueDB.Find(id);
            db.FFLeagueDB.Remove(fFLeague);
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
