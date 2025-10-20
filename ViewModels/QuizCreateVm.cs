using System.Collections.Generic;
using System.Web.Mvc;

namespace paradise.ViewModels
{
    public class QuizCreateVm
    {
        public long topic { get; set; }
        public string title { get; set; }
        public decimal? time { get; set; }
        public bool is_infinity { get; set; }
        public int quantity { get; set; }

        public List<QuestionVm> Questions { get; set; } = new List<QuestionVm>();
    }

    public class QuestionVm
    {
        public long? id { get; set; }

        // multiple_choice | essay
        public string question_type { get; set; } = "multiple_choice";

        [AllowHtml]
        public string question_text { get; set; }

        public int? CorrectOption { get; set; }

        // Dùng chung OptionVm
        public List<OptionVm> Options { get; set; } = new List<OptionVm>();

        public bool _remove { get; set; }
    }
}
