namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class quiz_attempts
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz_attempts()
        {
            quiz_attempt_answers = new HashSet<quiz_attempt_answers>();
        }

        public long id { get; set; }

        public long user_id { get; set; }

        public long quiz_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime started_at { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime finished_at { get; set; }

        public double score { get; set; }

        public virtual lesson_quizzes lesson_quizzes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempt_answers> quiz_attempt_answers { get; set; }

        public virtual user user { get; set; }
    }
}
