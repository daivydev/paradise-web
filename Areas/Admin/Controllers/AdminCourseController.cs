using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using paradise.Models;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminCourseController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: Admin/AdminCourse
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            // Nếu search rỗng nhưng URL có ?search= thì redirect về URL sạch
            if (string.IsNullOrWhiteSpace(search))
            {
                if (Request.QueryString["search"] != null)
                    return RedirectToAction("Index");
            }

            var query = db.courses
                          .OrderByDescending(u => u.created_at)  // mới nhất lên đầu
                          .Include(c => c.topic)
                          .Include(c => c.user)
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c =>
                    c.course_title.Contains(s) ||
                    c.user.email.Contains(s) ||
                    c.topic.topic_name.Contains(s)
                );
            }

            // Sắp xếp mới nhất trước (ưu tiên created_at nếu có)
            query = query.OrderByDescending(c => c.created_at);

            var total = query.Count();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var list = query.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(list);
        }

        // GET: Admin/AdminCourse/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cours = db.courses
                          .Include(c => c.topic)
                          .Include(c => c.user)
                          .FirstOrDefault(c => c.id == id);

            if (cours == null) return HttpNotFound();
            return View(cours);
        }

        // GET: Admin/AdminCourse/Create
        public ActionResult Create()
        {
            ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name");
            ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email");
            return View();
        }

        // POST: Admin/AdminCourse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(cours cours)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate cơ bản
                    if (cours.price < 0)
                        ModelState.AddModelError("CustomError", "Giá không được âm.");

                    // (Tùy chọn) Chống trùng tiêu đề trong cùng 1 tác giả
                    if (db.courses.Any(x => x.course_title == cours.course_title && x.author_id == cours.author_id))
                        ModelState.AddModelError("CustomError", "Tiêu đề khóa học đã tồn tại cho tác giả này.");

                    if (ModelState.IsValid)
                    {
                        cours.created_at = DateTime.Now;
                        cours.updated_at = DateTime.Now;

                        db.courses.Add(cours);
                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CustomError", "Có lỗi khi tạo khóa học: " + ex.Message);
                }
            }

            // Load lại SelectList khi có lỗi
            ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", cours.topics_id);
            ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", cours.author_id);
            return View(cours);
        }

        // GET: Admin/AdminCourse/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cours = db.courses.Find(id);
            if (cours == null) return HttpNotFound();

            ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", cours.topics_id);
            ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", cours.author_id);
            return View(cours);
        }

        // POST: Admin/AdminCourse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(cours form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = db.courses.FirstOrDefault(c => c.id == form.id);
                    if (existing == null) return HttpNotFound();

                    if (form.price < 0)
                        ModelState.AddModelError("CustomError", "Giá không được âm.");

                    // (Tùy chọn) Chống trùng tiêu đề trong cùng 1 tác giả
                    if (db.courses.Any(x => x.id != form.id &&
                                            x.course_title == form.course_title &&
                                            x.author_id == form.author_id))
                    {
                        ModelState.AddModelError("CustomError", "Tiêu đề khóa học đã tồn tại cho tác giả này.");
                    }

                    if (ModelState.IsValid)
                    {
                        // Cập nhật các trường cho phép
                        existing.topics_id = form.topics_id;
                        existing.author_id = form.author_id;
                        existing.course_title = form.course_title;
                        existing.course_description = form.course_description;
                        existing.course_thumbnail = form.course_thumbnail;
                        existing.price = form.price;
                        existing.updated_at = DateTime.Now; // Không động vào created_at

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CustomError", "Có lỗi khi cập nhật khóa học: " + ex.Message);
                }
            }

            ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", form.topics_id);
            ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", form.author_id);
            return View(form);
        }

        // GET: Admin/AdminCourse/Delete/5  (match route, dùng popup modal nên không render view riêng)
        public ActionResult Delete(long id)
        {
            var item = db.courses.Find(id);
            if (item == null) return HttpNotFound();
            return RedirectToAction("Index");
        }

        // POST: Admin/AdminCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var item = db.courses.Find(id);
            if (item != null)
            {
                db.courses.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
        [HttpPost]
        public ActionResult UpdateVisibility(long id, bool isVisible)
        {
            var review = db.courses.Find(id);
            if (review == null)
            {
                return Json(new { success = false });
            }

            review.is_visible = isVisible;
            review.updated_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true, isVisible = review.is_visible });
        }

    }
}
