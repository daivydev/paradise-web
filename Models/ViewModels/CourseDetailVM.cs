using System.Collections.Generic;

namespace paradise.Models.ViewModels
{
    // ViewModel trang chi tiết khóa học
    public class CourseDetailVM
    {
        public CourseDetailVM()
        {
            Sections = new List<SectionVM>();
        }

        public long CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<SectionVM> Sections { get; set; }
        public string CurrentLessonId { get; set; }
    }

    public class SectionVM
    {
        public SectionVM()
        {
            Lessons = new List<LessonVM>();
        }

        public string Title { get; set; }            // "1. Bắt đầu"
        public List<LessonVM> Lessons { get; set; }
    }

    public class LessonVM
    {
        public LessonVM()
        {
            Quiz = new List<QuizItemVM>();
        }

        public string Id { get; set; }               // "l123"
        public string Title { get; set; }            // "Giới thiệu"
        public string Duration { get; set; }         // "05:12"
        public string VideoSrc { get; set; }         // "/Content/videos/intro.mp4"
        public string ContentHtml { get; set; }      // lorem hoặc html
        public List<QuizItemVM> Quiz { get; set; }
    }

    public class QuizItemVM
    {
        public QuizItemVM()
        {
            A = new List<string>();
        }

        public string Q { get; set; }
        public List<string> A { get; set; }          // đáp án
        public int Correct { get; set; }             // index đáp án đúng
    }
}
