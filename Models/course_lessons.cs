namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class course_lessons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public course_lessons()
        {
            lesson_contents = new HashSet<lesson_contents>();
            lesson_progress = new HashSet<lesson_progress>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        public long chapter_id { get; set; }

        [Required]
        [StringLength(255)]
        public string lesson_title { get; set; }

        public int display_order { get; set; }


        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }
        public bool is_visible { get; set; } = true;


        public virtual course_chapters course_chapters { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lesson_contents> lesson_contents { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lesson_progress> lesson_progress { get; set; }
    }
}
