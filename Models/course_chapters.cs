namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class course_chapters
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public course_chapters()
        {
            course_lessons = new HashSet<course_lessons>();
        }

        public long id { get; set; }

        public long course_id { get; set; }

        [Required]
        [StringLength(255)]
        public string chapter_title { get; set; }

        [Required]
        public string chapter_description { get; set; }

        public int display_order { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }

        public virtual cours cours { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_lessons> course_lessons { get; set; }
    }
}
