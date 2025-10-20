namespace paradise.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("quiz_attempt_answers")]
    public partial class quiz_attempt_answers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        [ForeignKey("quiz_attempts")]
        public long attempt_id { get; set; }

        [ForeignKey("quiz_questions")]
        public long question_id { get; set; }

        [ForeignKey("quiz_options")]
        public long selected_option_id { get; set; }

        public bool is_correct { get; set; }

        public virtual quiz_attempts quiz_attempts { get; set; }

        public virtual quiz_questions quiz_questions { get; set; }

        public virtual quiz_options quiz_options { get; set; }
    }
}
