using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using paradise.Models;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminLesson_contentsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin/AdminLesson_contents
        public ActionResult Index()
        {
            var lesson_contents = db.lesson_contents.Include(l => l.course_lessons);
            return View(lesson_contents.ToList());
        }

        // GET: Admin/AdminLesson_contents/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            lesson_contents lesson_contents = db.lesson_contents.Find(id);
            if (lesson_contents == null)
            {
                return HttpNotFound();
            }
            return View(lesson_contents);
        }

        // GET: Admin/AdminLesson_contents/Create
        public ActionResult Create()
        {
            ViewBag.lesson_id = new SelectList(db.course_lessons, "id", "lesson_title");
            return View();
        }

        // POST: Admin/AdminLesson_contents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,lesson_id,content_type,content_text,content_url,display_order,created_at")] lesson_contents lesson_contents)
        {
            if (ModelState.IsValid)
            {
                db.lesson_contents.Add(lesson_contents);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.lesson_id = new SelectList(db.course_lessons, "id", "lesson_title", lesson_contents.lesson_id);
            return View(lesson_contents);
        }

        // GET: Admin/AdminLesson_contents/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            lesson_contents lesson_contents = db.lesson_contents.Find(id);
            if (lesson_contents == null)
            {
                return HttpNotFound();
            }
            ViewBag.lesson_id = new SelectList(db.course_lessons, "id", "lesson_title", lesson_contents.lesson_id);
            return View(lesson_contents);
        }

        // POST: Admin/AdminLesson_contents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,lesson_id,content_type,content_text,content_url,display_order,created_at")] lesson_contents lesson_contents)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lesson_contents).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.lesson_id = new SelectList(db.course_lessons, "id", "lesson_title", lesson_contents.lesson_id);
            return View(lesson_contents);
        }

        // GET: Admin/AdminLesson_contents/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            lesson_contents lesson_contents = db.lesson_contents.Find(id);
            if (lesson_contents == null)
            {
                return HttpNotFound();
            }
            return View(lesson_contents);
        }

        // POST: Admin/AdminLesson_contents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            lesson_contents lesson_contents = db.lesson_contents.Find(id);
            db.lesson_contents.Remove(lesson_contents);
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
