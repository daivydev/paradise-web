using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using paradise.Models;
using paradise.Models.ViewModels;

namespace paradise.Controllers
{
    public class CoursesController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // ===================== NEW: DANH SÁCH KHÓA HỌC =====================
        // /Courses?q=...&topicId=...&page=1&pageSize=12
        public ActionResult Index(string q = "", int? topicId = null, int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 48) pageSize = 12;

            var query = db.courses.AsQueryable();

            // chỉ hiển thị khóa học được bật
            query = query.Where(c => c.is_visible == true);

            if (topicId.HasValue)
                query = query.Where(c => c.topics_id == topicId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.course_title.Contains(q) ||
                    (c.course_description ?? "").Contains(q));
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(c => c.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListItemVM
                {
                    Id = (int)c.id,
                    TopicId = (int?)c.topics_id,
                    Title = c.course_title,
                    Thumbnail = c.course_thumbnail,
                    ShortDescription = c.course_description,
                    Price = c.price
                })
                .ToList();

            // dropdown chủ đề (nếu có bảng topics)
            var topics = db.topics
                .OrderBy(t => t.topic_name)
                .Select(t => new SelectListItem
                {
                    Value = t.id.ToString(),
                    Text = t.topic_name,
                    Selected = topicId.HasValue && t.id == topicId.Value
                })
                .ToList();

            var vm = new CourseListVM
            {
                Items = items,
                Q = q,
                TopicId = topicId,
                Page = page,
                Total = total,
                Topics = topics
            };

            return View(vm); // Views/Courses/Index.cshtml
        }
        // =================== END NEW: DANH SÁCH KHÓA HỌC ===================


        public ActionResult Detail(long id, string lessonId = null)
        {
            var course = db.courses.FirstOrDefault(c => c.id == id);
            if (course == null) return HttpNotFound();

            var chapters = db.course_chapters
                .Where(ch => ch.course_id == id)
                .OrderBy(ch => (int?)ch.display_order ?? int.MaxValue)
                .ThenBy(ch => ch.id)
                .ToList();

            var chapterIds = chapters.Select(ch => ch.id).ToList();

            var lessons = db.course_lessons
                .Where(ls => chapterIds.Contains(ls.chapter_id))
                .OrderBy(ls => (int?)ls.display_order ?? int.MaxValue)
                .ThenBy(ls => ls.id)
                .ToList();

            var lessonIds = lessons.Select(x => x.id).ToList();
            var contents = db.lesson_contents
                .Where(ct => lessonIds.Contains((long)ct.lesson_id))
                .OrderBy(ct => (int?)ct.display_order ?? int.MaxValue)
                .ThenBy(ct => ct.id)
                .ToList();

            var vm = new CourseDetailVM
            {
                CourseId = course.id,
                CourseTitle = course.course_title
            };

            for (int i = 0; i < chapters.Count; i++)
            {
                var ch = chapters[i];
                var sec = new SectionVM
                {
                    Title = $"{i + 1}. {(string.IsNullOrEmpty(ch.chapter_title) ? "Chương" : ch.chapter_title)}"
                };

                var chOrder = (ch.display_order > 0) ? ch.display_order : (i + 1);
                var lsInChapter = lessons.Where(l => l.chapter_id == ch.id).ToList();

                for (int li = 0; li < lsInChapter.Count; li++)
                {
                    var l = lsInChapter[li];
                    var lsOrder = (l.display_order > 0) ? l.display_order : (li + 1);

                    var html = contents
                        .Where(ct => ct.lesson_id == l.id)
                        .Select(ct => ct.content_text)
                        .FirstOrDefault();

                    sec.Lessons.Add(new LessonVM
                    {
                        Id = "l" + l.id,
                        Title = string.IsNullOrEmpty(l.lesson_title) ? "Bài học" : l.lesson_title,
                        Duration = ToDurationString(300),
                        VideoSrc = ResolveVideoSrc(course.id, chOrder, lsOrder, l.id),
                        ContentHtml = !string.IsNullOrEmpty(html)
                            ? html
                            : "<h3>Nội dung</h3><p>Đang cập nhật…</p>",
                        Quiz = new List<QuizItemVM> {
                            new QuizItemVM {
                                Q = "Câu hỏi mẫu?",
                                A = new List<string> { "Đáp án A", "Đáp án B", "Đáp án C" },
                                Correct = 1
                            }
                        }
                    });
                }

                vm.Sections.Add(sec);
            }

            vm.CurrentLessonId = lessonId;
            return View(vm);
        }

        private string ResolveVideoSrc(long courseId, int? chOrder, int? lsOrder, long lessonId)
        {
            const string ROOT = "/Content/videos";
            string Map(string v) => Server.MapPath(v);

            var candidates = new List<string>();
            candidates.Add($"{ROOT}/c{courseId}/l{lessonId}.mp4");
            if (chOrder.HasValue && lsOrder.HasValue)
                candidates.Add($"{ROOT}/c{courseId}/ch{chOrder.Value:00}-l{lsOrder.Value:00}.mp4");
            if (chOrder.HasValue && lsOrder.HasValue)
                candidates.Add($"{ROOT}/c{courseId}/c{chOrder.Value}l{lsOrder.Value}.mp4");
            candidates.Add($"{ROOT}/c{courseId}/course.mp4");
            candidates.Add($"{ROOT}/sample.mp4");

            foreach (var rel in candidates)
                if (System.IO.File.Exists(Map(rel))) return rel.Replace("~", "");

            return candidates.First().Replace("~", "");
        }

        private static string ToDurationString(int seconds)
        {
            if (seconds <= 0) return "00:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
        }
    }
}
