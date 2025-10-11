using paradise.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
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

            var course = db.courses
                .Include(c => c.topic)
                .Include(c => c.user)
                .Include(c => c.course_chapters.Select(ch => ch.course_lessons.Select(ls => ls.lesson_contents)))
                .FirstOrDefault(c => c.id == id);

            if (course == null) return HttpNotFound();

            course.course_chapters = course.course_chapters
                .OrderBy(ch => ch.display_order)
                .ThenBy(ch => ch.id)
                .ToList();

            foreach (var ch in course.course_chapters)
            {
                ch.course_lessons = ch.course_lessons
                    .OrderBy(ls => ls.display_order)
                    .ThenBy(ls => ls.id)
                    .ToList();

                foreach (var ls in ch.course_lessons)
                {
                    ls.lesson_contents = ls.lesson_contents
                        .OrderBy(ct => ct.display_order)
                        .ThenBy(ct => ct.id)
                        .ToList();
                }
            }

            return View(course);
        }



        // GET: Admin/AdminCourse/Create
        [HttpGet]
        public ActionResult Create()
        {
            
                //Tạo dropdown
                ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name");
                ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email");

                var vm = new CourseFullCreateVm();
                // seed 1 chapter > 1 lesson > 1 content để form có sẵn
                vm.Chapters.Add(new ChapterVm
                {
                    Lessons = { new LessonVm { Contents = { new LessonContentVm() } } }
                });

                return View(vm);
          
            
        }

        // POST: Admin/AdminCourse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseFullCreateVm vm)
        {
            // Lọc bỏ các item rỗng trước khi validate
            PruneEmptyNodes(vm);

            // Validate lại từ đầu trên dữ liệu đã được prune
            ModelState.Clear();
            TryValidateModel(vm);

            // Validate bổ sung theo nghiệp vụ
           

            if (!string.IsNullOrWhiteSpace(vm.course_title) &&
                db.courses.Any(x => x.course_title.Trim().ToLower() == vm.course_title.Trim().ToLower()
                                    && x.author_id == vm.author_id))
            {
                ModelState.AddModelError("course_title", "Tiêu đề khóa học đã tồn tại cho tác giả này.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", vm.topics_id);
                ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", vm.author_id);
                return View(vm);
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    // 1) Course
                    var course = new cours
                    {
                        topics_id = vm.topics_id,
                        author_id = vm.author_id,
                        course_title = vm.course_title?.Trim(),
                        course_thumbnail = vm.course_thumbnail,
                        price = vm.price,
                        course_description = vm.course_description,
                        is_visible = vm.is_visible,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };
                    db.courses.Add(course);
                    db.SaveChanges(); // có course.id

                    // 2) Chapters
                    int chOrd = 1;
                    foreach (var ch in vm.Chapters ?? new List<ChapterVm>())
                    {
                        var chapter = new course_chapters
                        {
                            course_id = course.id,
                            chapter_title = ch.chapter_title.Trim(),
                            chapter_description = ch.chapter_description,
                            display_order = ch.display_order ?? chOrd++,
                            is_visible = ch.is_visible,              
                            created_at = DateTime.Now
                        };
                        db.course_chapters.Add(chapter);
                        db.SaveChanges(); // có chapter.id

                        // 3) Lessons
                        int lsOrd = 1;
                        foreach (var ls in ch.Lessons ?? new List<LessonVm>())
                        {
                            var lesson = new course_lessons
                            {
                                chapter_id = chapter.id,
                                lesson_title = ls.lesson_title.Trim(),
                                display_order = ls.display_order ?? lsOrd++,
                                is_visible = ls.is_visible,              
                                created_at = DateTime.Now
                            };
                            db.course_lessons.Add(lesson);
                            db.SaveChanges(); // có lesson.id

                            // 4) Contents
                            int ctOrd = 1;
                            foreach (var ct in ls.Contents ?? new List<LessonContentVm>())
                            {
                                var content = new lesson_contents
                                {
                                    lesson_id = lesson.id,
                                    content_type = (ct.content_type ?? "").Trim(), // bạn có thể set theo extension ở dưới
                                    content_text = (ct.title ?? "").Trim(),
                                    // content_url sẽ set sau khi xử lý file
                                    display_order = (ct.display_order.HasValue && ct.display_order.Value > 0)
        ? ct.display_order.Value : ctOrd++,
                                    is_visible = ct.is_visible,
                                    created_at = DateTime.Now
                                };

                                // Xử lý file nếu có
                                if (ct.upload_file != null && ct.upload_file.ContentLength > 0)
                                {
                                    var ext = System.IO.Path.GetExtension(ct.upload_file.FileName) ?? "";
                                    var safeName = $"{Guid.NewGuid():N}{ext}";
                                    var saveDir = Server.MapPath("~/Uploads/LessonContents");
                                    System.IO.Directory.CreateDirectory(saveDir);
                                    var savePath = System.IO.Path.Combine(saveDir, safeName);
                                    ct.upload_file.SaveAs(savePath);

                                    content.content_url = "/Uploads/LessonContents/" + safeName;

                                    // (tuỳ chọn) tự gán content_type theo đuôi file
                                    if (string.IsNullOrWhiteSpace(content.content_type))
                                    {
                                        content.content_type =
                                            new[] { ".mp4", ".mov", ".mkv" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "video" :
                                            new[] { ".pdf" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "pdf" :
                                            new[] { ".doc", ".docx" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "doc" :
                                            new[] { ".xlsx", ".xls" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "excel" :
                                            new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "image" :
                                            "file";
                                    }
                                }
                                else
                                {
                                    // Không có file: nếu bạn vẫn muốn hỗ trợ URL text, dùng content_body làm URL
                                    // (Nếu bạn không cần nữa, có thể bỏ nhánh này)
                                    content.content_url = ct.content_body;
                                }

                                db.lesson_contents.Add(content);
                            }
                            db.SaveChanges();
                        }
                    }

                    tx.Commit();
                    TempData["Success"] = "Tạo đầy đủ Course + Chapter + Lesson + Content thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError("CustomError", "Lỗi khi lưu: " + ex.Message);

                    ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", vm.topics_id);
                    ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", vm.author_id);
                    return View(vm);
                }
            }
        }

        // GET: Admin/AdminCourse/Edit/5
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            try
            {
                var course = db.courses.FirstOrDefault(c => c.id == id);
                if (course == null) return HttpNotFound();

                // Lấy danh sách chương (theo thứ tự display_order)
                var chapters = db.course_chapters.Where(x => x.course_id == course.id)
                                   .OrderBy(x => x.display_order).ThenBy(x => x.id).ToList();

                // Lấy danh sách bài thuộc các chương đó
                var chIds = chapters.Select(x => x.id).ToList();
                var lessons = db.course_lessons.Where(x => chIds.Contains(x.chapter_id))
                                   .OrderBy(x => x.display_order).ThenBy(x => x.id).ToList();

                // Lấy danh sách content thuộc các bài
                var lsIds = lessons.Select(x => x.id).ToList();
                var contents = db.lesson_contents.Where(x => lsIds.Contains(x.lesson_id))
                                   .OrderBy(x => x.display_order).ThenBy(x => x.id).ToList();

                var vm = new CourseFullCreateVm
                {
                    id = course.id,
                    topics_id = course.topics_id,
                    author_id = course.author_id,
                    course_title = course.course_title,
                    course_thumbnail = course.course_thumbnail,
                    price = course.price,
                    course_description = course.course_description,
                    is_visible = course.is_visible,

                    // Chuyển chapters -> ChapterVm
                    Chapters = chapters.Select(ch => new ChapterVm
                    {
                        chapter_title = ch.chapter_title,
                        chapter_description = ch.chapter_description,
                        display_order = ch.display_order,

                        // Lồng lesson vào chapter
                        Lessons = lessons.Where(ls => ls.chapter_id == ch.id)
                                         .Select(ls => new LessonVm
                                         {
                                             lesson_title = ls.lesson_title,
                                             display_order = ls.display_order,

                                             // Lồng content vào lesson
                                             Contents = contents.Where(ct => ct.lesson_id == ls.id)
                                                                 .Select(ct => new LessonContentVm
                                                                 {
                                                                     title = ct.content_text,
                                                                     content_type = ct.content_type,
                                                                     content_body = ct.content_url,
                                                                     display_order = ct.display_order
                                                                 }).ToList()
                                         }).ToList()
                    }).ToList()
                };

                ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", vm.topics_id);
                ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", vm.author_id);

                return View(vm);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                    "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message);
            }
        }


        // POST: Admin/AdminCourse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CourseFullCreateVm vm)
        {
            // Lọc bỏ các item rỗng trước khi validate
            PruneEmptyNodes(vm);

            // Validate lại từ đầu trên dữ liệu đã được prune
            ModelState.Clear();
            TryValidateModel(vm);

            // ---- Validate cơ bản ----
            if (vm.price < 1000)
                ModelState.AddModelError("price", "Giá phải từ 1000 trở lên.");

            if (!string.IsNullOrWhiteSpace(vm.course_title) &&
                db.courses.Any(x => x.id != vm.id &&
                                    x.course_title == vm.course_title &&
                                    x.author_id == vm.author_id))
                ModelState.AddModelError("course_title", "Tiêu đề khóa học đã tồn tại cho tác giả này.");

            if (!ModelState.IsValid)
            {
                ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", vm.topics_id);
                ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", vm.author_id);
                return View(vm); // trả lại view Edit.cshtml với VM
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    // 1) Tìm course
                    var course = db.courses.FirstOrDefault(c => c.id == vm.id);
                    if (course == null) return HttpNotFound();

                    // 2) Cập nhật course
                    course.topics_id = vm.topics_id;
                    course.author_id = vm.author_id;
                    course.course_title = vm.course_title?.Trim();
                    course.course_thumbnail = vm.course_thumbnail;
                    course.price = vm.price;
                    course.course_description = vm.course_description;
                    course.is_visible = vm.is_visible;
                    course.updated_at = DateTime.Now;
                    db.SaveChanges();

                    // 3) Xóa cây con cũ (nếu chưa bật cascade thì xóa theo thứ tự)
                    var chIds = db.course_chapters.Where(x => x.course_id == course.id).Select(x => x.id).ToList();
                    var lsIds = db.course_lessons.Where(x => chIds.Contains(x.chapter_id)).Select(x => x.id).ToList();

                    db.lesson_contents.RemoveRange(db.lesson_contents.Where(x => lsIds.Contains(x.lesson_id)));
                    db.course_lessons.RemoveRange(db.course_lessons.Where(x => chIds.Contains(x.chapter_id)));
                    db.course_chapters.RemoveRange(db.course_chapters.Where(x => x.course_id == course.id));
                    db.SaveChanges();

                    // 4) Chèn lại theo VM

                    //Chapter (Chương)
                    int chOrd = 1; //gán thứ tự bắt đầu 1
                    foreach (var ch in vm.Chapters ?? new List<ChapterVm>())
                    {
                        if (string.IsNullOrWhiteSpace(ch.chapter_title)) continue;

                        var chapter = new course_chapters
                        {
                            course_id = course.id,
                            chapter_title = ch.chapter_title.Trim(),
                            chapter_description = ch.chapter_description,
                            display_order = ch.display_order ?? chOrd++,    //nếu tạo thêm 1 chương sẽ tự động + thêm 1 giá trị cho thứ tự chương
                            is_visible = ch.is_visible,             
                            created_at = DateTime.Now
                        };
                        db.course_chapters.Add(chapter);
                        db.SaveChanges();

                        //Lesson (Bài)
                        int lsOrd = 1; //gán thứ tự bài là 1
                        foreach (var ls in ch.Lessons ?? new List<LessonVm>())
                        {
                            if (string.IsNullOrWhiteSpace(ls.lesson_title)) continue;

                            var lesson = new course_lessons
                            {
                                chapter_id = chapter.id,
                                lesson_title = ls.lesson_title.Trim(),
                                display_order = ls.display_order ?? lsOrd++,    //nếu tạo thêm 1 bài thì sẽ tự động + thêm 1 giá trị thứ tự cho bài
                                is_visible = ls.is_visible,              
                                created_at = DateTime.Now
                            };
                            db.course_lessons.Add(lesson);
                            db.SaveChanges();

                            //Content (Nội dung)
                            int ctOrd = 1;  //gán thứ tự nội dung = 1
                            foreach (var ct in ls.Contents ?? new List<LessonContentVm>())
                            {
                                if (string.IsNullOrWhiteSpace(ct.title) && string.IsNullOrWhiteSpace(ct.content_body))
                                    continue;

                                var content = new lesson_contents
                                {   
                                    lesson_id = lesson.id,
                                    content_type = (ct.content_type ?? "").Trim(),
                                    content_text = (ct.title ?? "").Trim(),
                                    display_order = (ct.display_order.HasValue && ct.display_order.Value > 0)? ct.display_order.Value : ctOrd++,
                                    is_visible = ct.is_visible,
                                    created_at = DateTime.Now
                                };

                                // Xử lý file nếu có
                                if (ct.upload_file != null && ct.upload_file.ContentLength > 0)
                                {
                                    var ext = System.IO.Path.GetExtension(ct.upload_file.FileName) ?? "";
                                    var safeName = $"{Guid.NewGuid():N}{ext}";
                                    var saveDir = Server.MapPath("~/Uploads/LessonContents");
                                    System.IO.Directory.CreateDirectory(saveDir);
                                    var savePath = System.IO.Path.Combine(saveDir, safeName);
                                    ct.upload_file.SaveAs(savePath);

                                    content.content_url = "/Uploads/LessonContents/" + safeName;

                                    if (string.IsNullOrWhiteSpace(content.content_type))
                                    {
                                        content.content_type =
                                            new[] { ".mp4", ".mov", ".mkv" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "video" :
                                            new[] { ".pdf" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "pdf" :
                                            new[] { ".doc", ".docx" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "doc" :
                                            new[] { ".xlsx", ".xls" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "excel" :
                                            new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }.Contains(ext, StringComparer.OrdinalIgnoreCase) ? "image" :
                                            "file";
                                    }
                                }
                                else
                                {
                                    // không upload mới -> dùng URL/text sẵn có
                                    content.content_url = ct.content_body;
                                }

                                db.lesson_contents.Add(content);

                            }
                            db.SaveChanges();
                        }
                    }

                    tx.Commit();
                    TempData["Success"] = "Cập nhật khóa học (Course + Chapter + Lesson + Content) thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError("CustomError", "Lỗi khi cập nhật: " + ex.Message);

                    ViewBag.topics_id = new SelectList(db.topics.ToList(), "id", "topic_name", vm.topics_id);
                    ViewBag.author_id = new SelectList(db.users.ToList(), "id", "email", vm.author_id);
                    return View(vm);
                }
            }
        }

        // GET: Admin/AdminCourse/Delete/5 
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
        /* ----------------- Helpers: loại bỏ node rỗng trước khi validate ----------------- */
        private static bool IsBlankContent(LessonContentVm c)
        {
            // rỗng khi cả title và content_body đều trống
            return c == null ||
                   (string.IsNullOrWhiteSpace(c.title) && string.IsNullOrWhiteSpace(c.content_body));
        }

        private static bool IsBlankLesson(LessonVm l)
        {
            // coi là rỗng nếu không có tiêu đề và tất cả contents đều rỗng
            if (l == null) return true;
            var noTitle = string.IsNullOrWhiteSpace(l.lesson_title);
            var allContentsBlank = (l.Contents == null) || l.Contents.All(IsBlankContent);
            return noTitle && allContentsBlank;
        }

        private static bool IsBlankChapter(ChapterVm ch)
        {
            // coi là rỗng nếu không có tiêu đề và tất cả lessons đều rỗng
            if (ch == null) return true;
            var noTitle = string.IsNullOrWhiteSpace(ch.chapter_title);
            var allLessonsBlank = (ch.Lessons == null) || ch.Lessons.All(IsBlankLesson);
            return noTitle && allLessonsBlank;
        }

        private static void PruneEmptyNodes(CourseFullCreateVm vm)
        {
            if (vm == null) return;

            // Lọc content rỗng
            foreach (var ch in vm.Chapters ?? new List<ChapterVm>())
            {
                foreach (var ls in ch.Lessons ?? new List<LessonVm>())
                {
                    if (ls.Contents != null)
                        ls.Contents = ls.Contents.Where(c => !IsBlankContent(c)).ToList();
                }

                if (ch.Lessons != null)
                    ch.Lessons = ch.Lessons.Where(l => !IsBlankLesson(l)).ToList();
            }

            // Lọc chapter rỗng
            if (vm.Chapters != null)
                vm.Chapters = vm.Chapters.Where(c => !IsBlankChapter(c)).ToList();
        }
    }
}
