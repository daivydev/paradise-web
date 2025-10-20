using System.Collections.Generic;
using System.Web.Mvc;

namespace paradise.ViewModels
{
    public class QuizEditVm
    {
        public long id { get; set; }
        public string title { get; set; }
        public long topic { get; set; }
        public decimal? time { get; set; }
        public bool is_infinity { get; set; }
        public int quantity { get; set; }

        public List<QuestionEditVm> Questions { get; set; } = new List<QuestionEditVm>();
    }

    public class QuestionEditVm
    {
        public long? id { get; set; }

        [AllowHtml]
        public string question_text { get; set; }

        public int? CorrectOption { get; set; }

        public List<OptionEditVm> Options { get; set; } = new List<OptionEditVm>();

        // flag dùng khi xoá câu hỏi trên UI
        public bool _remove { get; set; }
    }

    public class OptionEditVm
    {
        public long? id { get; set; }

        [AllowHtml]
        public string option_text { get; set; }

        // flag dùng khi xoá đáp án trên UI
        public bool _remove { get; set; }
    }
}
