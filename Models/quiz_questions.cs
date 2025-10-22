namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class quiz_questions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz_questions()
        {
            quiz_attempt_answers = new HashSet<quiz_attempt_answers>();
            quiz_options = new HashSet<quiz_options>();
        }

        public long id { get; set; }

        public long quiz_id { get; set; }

        [Required]
        public string question_text { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }

        public string question_type { get; set; }

        public virtual quiz quiz { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempt_answers> quiz_attempt_answers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_options> quiz_options { get; set; }
    }
}
