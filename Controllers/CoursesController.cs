using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using paradise.Models;
using paradise.Models.ViewModels;
using paradise.Helpers;

namespace paradise.Controllers
{
    public class CoursesController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // ===================== DANH SÁCH KHÓA HỌC =====================
        public ActionResult Index(string q = "", int? topicId = null, int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 48) pageSize = 12;

            var query = db.courses.AsQueryable()
                                  .Where(c => c.is_visible == true);

            if (topicId.HasValue)
                query = query.Where(c => c.topics_id == topicId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c => c.course_title.Contains(q) ||
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

            return View(vm);
        }
        // =================== END: DANH SÁCH KHÓA HỌC ===================


        // ===================== CHI TIẾT KHÓA HỌC =====================
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
                .Where(ct => lessonIds.Contains((long)ct.lesson_id) && ct.is_visible == true)
                .OrderBy(ct => (int?)ct.display_order ?? int.MaxValue)
                .ThenBy(ct => ct.id)
                .ToList();

            var contentsByLesson = contents
                .GroupBy(c => c.lesson_id)
                .ToDictionary(g => g.Key, g => g.ToList());

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

                    // ==== Ưu tiên: YOUTUBE -> VIDEO -> DOCX -> TEXT ====
                    var list = contentsByLesson.ContainsKey(l.id) ? contentsByLesson[l.id] : new List<lesson_contents>();
                    var cYouTube = list.FirstOrDefault(x => IsYouTubeType(x.content_type) || LooksLikeYouTube(x.content_url));
                    var cVideo = list.FirstOrDefault(x => IsVideoType(x.content_type));
                    var cDocx = list.FirstOrDefault(x =>
                                      !string.IsNullOrWhiteSpace(x.content_url) &&
                                      x.content_url.EndsWith(".docx", StringComparison.OrdinalIgnoreCase));
                    var cText = list.FirstOrDefault(x => IsTextType(x.content_type) && !string.IsNullOrWhiteSpace(x.content_text));

                    string kind = "video";
                    string videoSrc = null;
                    string youtubeId = null;
                    string html = null;

                    if (cYouTube != null)
                    {
                        kind = "youtube";
                        youtubeId = ExtractYoutubeId(cYouTube.content_url);
                        // nếu có text kèm theo thì show dưới player
                        html = cText?.content_text;
                    }
                    else if (cVideo != null)
                    {
                        kind = "video";
                        videoSrc = BuildFileUrl(cVideo.content_url);
                        html = cText?.content_text;
                    }
                    else if (cDocx != null)
                    {
                        kind = "text";
                        // Convert DOCX -> HTML
                        var docxRel = BuildFileUrl(cDocx.content_url); // đảm bảo dạng "/Uploads/.."
                        html = DocxToHtmlHelper.ConvertRelativeDocxToHtml(Server, docxRel);
                    }
                    else if (cText != null)
                    {
                        kind = "text";
                        html = cText.content_text;
                    }

                    sec.Lessons.Add(new LessonVM
                    {
                        Id = "l" + l.id,
                        Title = string.IsNullOrEmpty(l.lesson_title) ? "Bài học" : l.lesson_title,
                        Duration = ToDurationString(300),

                        Kind = kind,          // "video" | "youtube" | "text"
                        VideoSrc = videoSrc,
                        YoutubeId = youtubeId,
                        ContentHtml = html ?? ""
                    });
                }

                vm.Sections.Add(sec);
            }

            vm.CurrentLessonId = lessonId;
            return View(vm);
        }
        // =================== END: CHI TIẾT KHÓA HỌC ===================


        // -------- Helpers nhận diện loại nội dung --------
        private static bool IsVideoType(string t)
            => !string.IsNullOrWhiteSpace(t) && t.Trim().Equals("VIDEO", StringComparison.OrdinalIgnoreCase);

        private static bool IsTextType(string t)
            => !string.IsNullOrWhiteSpace(t) && t.Trim().Equals("TEXT", StringComparison.OrdinalIgnoreCase);

        private static bool IsYouTubeType(string t)
            => !string.IsNullOrWhiteSpace(t) &&
               (t.Trim().Equals("YOUTUBE", StringComparison.OrdinalIgnoreCase)
                || t.Trim().Equals("LINK", StringComparison.OrdinalIgnoreCase));

        private static bool LooksLikeYouTube(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            var s = url.Trim();
            return s.IndexOf("youtube.com", StringComparison.OrdinalIgnoreCase) >= 0
                || s.IndexOf("youtu.be", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Chuẩn hoá đường dẫn web của file upload
        private static string BuildFileUrl(string contentUrl)
        {
            if (string.IsNullOrWhiteSpace(contentUrl)) return null;

            var s = contentUrl.Trim();
            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("/", StringComparison.Ordinal))
            {
                return s;
            }
            // chỉ là file-name -> map vào thư mục Uploads
            return "/Uploads/LessonContents/" + s;
        }

        private static string ExtractYoutubeId(string urlOrId)
        {
            if (string.IsNullOrWhiteSpace(urlOrId)) return null;
            var s = urlOrId.Trim();

            var idx = s.IndexOf("youtu.be/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return s.Substring(idx + "youtu.be/".Length).Split('?', '#', '&')[0];

            idx = s.IndexOf("watch?v=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return s.Substring(idx + "watch?v=".Length).Split('&', '#', '?')[0];

            idx = s.IndexOf("/embed/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return s.Substring(idx + "/embed/".Length).Split('?', '#', '&')[0];

            return s;
        }

        private static string ToDurationString(int seconds)
        {
            if (seconds <= 0) return "00:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
        }
    }
}
