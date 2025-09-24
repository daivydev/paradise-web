namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class lesson_quizzes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public lesson_quizzes()
        {
            quiz_attempts = new HashSet<quiz_attempts>();
            quiz_questions = new HashSet<quiz_questions>();
        }

        public long id { get; set; }

        public long lesson_id { get; set; }

        [Required]
        [StringLength(255)]
        public string quiz_title { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }

        public virtual course_lessons course_lessons { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempts> quiz_attempts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_questions> quiz_questions { get; set; }
    }
}
