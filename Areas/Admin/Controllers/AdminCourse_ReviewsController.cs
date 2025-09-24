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
    public class AdminCourse_ReviewsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin/AdminCourse_Reviews
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
            var query = db.course_reviews.Include(c => c.cours).Include(c => c.user).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.cours.course_title.Contains(search) ||            // tìm theo tên khóa học
                    e.user.email.Contains(search) ||             // tìm theo email user
                    e.rating.ToString().Contains(search) ||     // tìm theo rating
                    e.review_text.Contains(search)   // tìm theo review_text
                );
            }

            var course_reviews = query.OrderBy(e => e.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            int total = query.Count();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;

            return View(course_reviews);
        }

        [HttpPost]
        public ActionResult UpdateVisibility(long id, bool isVisible)
        {
            var review = db.course_reviews.Find(id);
            if (review == null)
            {
                return Json(new { success = false });
            }

            review.is_visible = isVisible;
            review.updated_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true, isVisible = review.is_visible });
        }

        // GET: Admin/AdminCourse_Reviews/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_reviews course_reviews = db.course_reviews.Find(id);
            if (course_reviews == null)
            {
                return HttpNotFound();
            }
            return View(course_reviews);
        }

        // POST: Admin/AdminCourse_Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            course_reviews course_reviews = db.course_reviews.Find(id);
            db.course_reviews.Remove(course_reviews);
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
