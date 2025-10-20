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
    public class AdminCourse_lessonsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin/AdminCourse_lessons
        public ActionResult Index()
        {
            var course_lessons = db.course_lessons.Include(c => c.course_chapters);
            return View(course_lessons.ToList());
        }

        // GET: Admin/AdminCourse_lessons/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_lessons course_lessons = db.course_lessons.Find(id);
            if (course_lessons == null)
            {
                return HttpNotFound();
            }
            return View(course_lessons);
        }

        // GET: Admin/AdminCourse_lessons/Create
        public ActionResult Create()
        {
            ViewBag.chapter_id = new SelectList(db.course_chapters, "id", "chapter_title");
            return View();
        }

        // POST: Admin/AdminCourse_lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,chapter_id,lesson_title,display_order,created_at")] course_lessons course_lessons)
        {
            if (ModelState.IsValid)
            {
                db.course_lessons.Add(course_lessons);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.chapter_id = new SelectList(db.course_chapters, "id", "chapter_title", course_lessons.chapter_id);
            return View(course_lessons);
        }

        // GET: Admin/AdminCourse_lessons/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_lessons course_lessons = db.course_lessons.Find(id);
            if (course_lessons == null)
            {
                return HttpNotFound();
            }
            ViewBag.chapter_id = new SelectList(db.course_chapters, "id", "chapter_title", course_lessons.chapter_id);
            return View(course_lessons);
        }

        // POST: Admin/AdminCourse_lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,chapter_id,lesson_title,display_order,created_at")] course_lessons course_lessons)
        {
            if (ModelState.IsValid)
            {
                db.Entry(course_lessons).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.chapter_id = new SelectList(db.course_chapters, "id", "chapter_title", course_lessons.chapter_id);
            return View(course_lessons);
        }

        // GET: Admin/AdminCourse_lessons/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_lessons course_lessons = db.course_lessons.Find(id);
            if (course_lessons == null)
            {
                return HttpNotFound();
            }
            return View(course_lessons);
        }

        // POST: Admin/AdminCourse_lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            course_lessons course_lessons = db.course_lessons.Find(id);
            db.course_lessons.Remove(course_lessons);
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
