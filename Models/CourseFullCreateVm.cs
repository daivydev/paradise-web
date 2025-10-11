namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


    public class LessonContentVm
    {
        [Required] public string title { get; set; }
        public string content_type { get; set; } // text/video/pdf/link...
        public string content_body { get; set; } // URL hoặc HTML/markdown
        public int? display_order { get; set; }
        public bool is_visible { get; set; } = true;

        public System.Web.HttpPostedFileBase upload_file { get; set; }

    }

    public class LessonVm
    {
        [Required] public string lesson_title { get; set; }
        public int? display_order { get; set; }
        public bool is_visible { get; set; } = true;
        public List<LessonContentVm> Contents { get; set; } = new List<LessonContentVm>();
    }

    public class ChapterVm
    {
        [Required] public string chapter_title { get; set; }
        public string chapter_description { get; set; }   // -> course_chapters.chapter_description

        public int? display_order { get; set; }
        public bool is_visible { get; set; } = true;
        public List<LessonVm> Lessons { get; set; } = new List<LessonVm>();
    }

    public class CourseFullCreateVm
    {
        public long id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ đề")]
        public long topics_id { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        public long author_id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề khóa học")]
        public string course_title { get; set; }
        public string course_thumbnail { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, Double.MaxValue, ErrorMessage = "Giá phải từ 1000 trở lên")]
        public decimal price { get; set; }

        public string course_description { get; set; }
        public bool is_visible { get; set; } = true;

        // Tree 3 tầng con:
        public List<ChapterVm> Chapters { get; set; } = new List<ChapterVm>();
    }
}