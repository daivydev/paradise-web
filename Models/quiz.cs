namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("quiz")]
    public partial class quiz
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz()
        {
            quiz_attempts = new HashSet<quiz_attempts>();
            quiz_questions = new HashSet<quiz_questions>();
        }

        public long id { get; set; }

        [Required]
        [StringLength(255)]
        public string title { get; set; }

        public long topic { get; set; }

        public decimal? time { get; set; }

        public bool is_infinity { get; set; }

        public int quantity { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime updated_at { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempts> quiz_attempts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_questions> quiz_questions { get; set; }

        public virtual topic_quiz topic_quiz { get; set; }
    }
}
