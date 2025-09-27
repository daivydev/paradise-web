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
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                // Nếu search rỗng thì redirect về URL gốc không có query
                if (Request.QueryString["search"] != null)
                {
                    return RedirectToAction("Index");
                }
            }

            var query = db.course_lessons.Include(c => c.course_chapters).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.lesson_title.Contains(search) ||
                    c.course_chapters.chapter_title.Contains(search)
                );
            }

            // Phân trang
            var lessons = query
                .OrderBy(c => c.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalItems = query.Count();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;

            return View(lessons);
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
            var model = new course_lessons
            {
                created_at = DateTime.Now   // Gán mặc định hôm nay
            };

            ViewBag.chapter_id = new SelectList(db.course_chapters, "id", "chapter_title");
            return View(model);
        }

        // POST: Admin/AdminCourse_lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,chapter_id,lesson_title,display_order,created_at")] course_lessons course_lessons)
        {
            if (ModelState.IsValid)
            {
                if (course_lessons.created_at == default(DateTime))
                {
                    course_lessons.created_at = DateTime.Now;
                }

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
