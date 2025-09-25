using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using paradise.Models;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminCourse_chaptersController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: Admin/AdminCourse_chapters
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            // Nếu search rỗng nhưng URL có ?search= thì redirect về URL sạch
            if (string.IsNullOrWhiteSpace(search))
            {
                if (Request.QueryString["search"] != null)
                    return RedirectToAction("Index");
            }

            var query = db.course_chapters
                          .Include(x => x.cours)
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.chapter_title.Contains(s) ||
                    x.chapter_description.Contains(s) ||
                    x.cours.course_title.Contains(s)
                );
            }

            // Sắp xếp: theo course rồi display_order (nếu có), sau đó theo id
            query = query
                .OrderBy(x => x.course_id)
                .ThenBy(x => x.display_order)
                .ThenBy(x => x.id);

            var total = query.Count();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var list = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(list);
        }

        // GET: Admin/AdminCourse_chapters/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = db.course_chapters
                         .Include(x => x.cours)
                         .FirstOrDefault(x => x.id == id);

            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // GET: Admin/AdminCourse_chapters/Create
        public ActionResult Create()
        {
            ViewBag.course_id = new SelectList(db.courses.ToList(), "id", "course_title");
            return View();
        }

        // POST: Admin/AdminCourse_chapters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(course_chapters form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate cơ bản
                    if (form.display_order < 0)
                        ModelState.AddModelError("CustomError", "Thứ tự hiển thị không được âm.");

                    // Chống trùng tiêu đề trong cùng 1 khóa học (tùy chọn)
                    if (db.course_chapters.Any(x => x.course_id == form.course_id &&
                                                    x.chapter_title == form.chapter_title))
                    {
                        ModelState.AddModelError("CustomError", "Tiêu đề chương đã tồn tại trong khóa học này.");
                    }

                    if (ModelState.IsValid)
                    {
                        form.created_at = DateTime.Now; // set server-side
                        db.course_chapters.Add(form);
                        db.SaveChanges();

                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CustomError", "Có lỗi khi tạo chương: " + ex.Message);
                }
            }

            // Load lại SelectList khi có lỗi
            ViewBag.course_id = new SelectList(db.courses.ToList(), "id", "course_title", form.course_id);
            return View(form);
        }

        // GET: Admin/AdminCourse_chapters/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var item = db.course_chapters.Find(id);
            if (item == null)
                return HttpNotFound();

            ViewBag.course_id = new SelectList(db.courses.ToList(), "id", "course_title", item.course_id);
            return View(item);
        }

        // POST: Admin/AdminCourse_chapters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(course_chapters form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = db.course_chapters.FirstOrDefault(x => x.id == form.id);
                    if (existing == null)
                        return HttpNotFound();

                    if (form.display_order < 0)
                        ModelState.AddModelError("CustomError", "Thứ tự hiển thị không được âm.");

                    // (Tùy chọn) Chống trùng tiêu đề trong cùng 1 khóa học, ngoại trừ bản thân
                    if (db.course_chapters.Any(x => x.id != form.id &&
                                                    x.course_id == form.course_id &&
                                                    x.chapter_title == form.chapter_title))
                    {
                        ModelState.AddModelError("CustomError", "Tiêu đề chương đã tồn tại trong khóa học này.");
                    }

                    if (ModelState.IsValid)
                    {
                        // Cập nhật các trường cho phép
                        existing.course_id = form.course_id;
                        existing.chapter_title = form.chapter_title;
                        existing.chapter_description = form.chapter_description;
                        existing.display_order = form.display_order;
                        // Không sửa created_at tại Edit

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CustomError", "Có lỗi khi cập nhật chương: " + ex.Message);
                }
            }

            ViewBag.course_id = new SelectList(db.courses.ToList(), "id", "course_title", form.course_id);
            return View(form);
        }

        // GET: Admin/AdminCourse_chapters/Delete/5  (match route, dùng modal; không render view riêng)
        public ActionResult Delete(long id)
        {
            var item = db.course_chapters.Find(id);
            if (item == null) return HttpNotFound();
            return RedirectToAction("Index");
        }

        // POST: Admin/AdminCourse_chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var item = db.course_chapters.Find(id);
            if (item != null)
            {
                db.course_chapters.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
