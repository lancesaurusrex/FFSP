using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DAL;

namespace WebApplication1.Controllers
{
    public class NFLPlayersController : Controller
    {
        private FF db = new FF();

        // GET: NFLPlayers
        public ActionResult Index()
        {           
            return View(db.NFLPlayer.ToList());
        }

        // GET: NFLPlayers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFLPlayer nFLPlayer = db.NFLPlayer.Find(id);
            if (nFLPlayer == null)
            {
                return HttpNotFound();
            }
            return View(nFLPlayer);
        }

        // GET: NFLPlayers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NFLPlayers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,team,PassingStats,RushingStats,ReceivingStats,FumbleStats,KickingStats")] NFLPlayer nFLPlayer)
        {
            if (ModelState.IsValid)
            {
                db.NFLPlayer.Add(nFLPlayer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nFLPlayer);
        }

        // GET: NFLPlayers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFLPlayer nFLPlayer = db.NFLPlayer.Find(id);
            if (nFLPlayer == null)
            {
                return HttpNotFound();
            }
            return View(nFLPlayer);
        }

        // POST: NFLPlayers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,team,PassingStats,RushingStats,ReceivingStats,FumbleStats,KickingStats")] NFLPlayer nFLPlayer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nFLPlayer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nFLPlayer);
        }

        // GET: NFLPlayers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NFLPlayer nFLPlayer = db.NFLPlayer.Find(id);
            if (nFLPlayer == null)
            {
                return HttpNotFound();
            }
            return View(nFLPlayer);
        }

        // POST: NFLPlayers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            NFLPlayer nFLPlayer = db.NFLPlayer.Find(id);
            db.NFLPlayer.Remove(nFLPlayer);
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
