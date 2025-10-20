using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace paradise.Models
{
    public class QuizCreateVm
    {
        public string title { get; set; }
        public long topic { get; set; }
        public decimal? time { get; set; }
        public bool is_infinity { get; set; }
        public int quantity { get; set; }

        // Danh sách câu hỏi nhập từ view
        public List<QuestionVm> Questions { get; set; }
    }

    public class QuestionVm
    {
        [AllowHtml] public string question_text { get; set; }
        public List<OptionVm> Options { get; set; }
        public int? CorrectOption { get; set; }
    }

    public class OptionVm
    {
        [AllowHtml] public string option_text { get; set; }     // <-- cho HTML
    }
}
