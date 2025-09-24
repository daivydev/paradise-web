namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class quiz_attempt_answers
    {
        public long id { get; set; }

        public long attempt_id { get; set; }

        public long question_id { get; set; }

        public long selected_option_id { get; set; }

        public bool is_correct { get; set; }

        public virtual quiz_attempts quiz_attempts { get; set; }

        public virtual quiz_options quiz_options { get; set; }

        public virtual quiz_questions quiz_questions { get; set; }
    }
}
