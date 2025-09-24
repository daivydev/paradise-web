namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("courses")]
    public partial class cours
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cours()
        {
            course_chapters = new HashSet<course_chapters>();
            course_enrollments = new HashSet<course_enrollments>();
            course_reviews = new HashSet<course_reviews>();
        }

        public long id { get; set; }

        public long topics_id { get; set; }

        public long author_id { get; set; }

        [Required]
        [StringLength(255)]
        public string course_title { get; set; }

        public string course_description { get; set; }

        [StringLength(255)]
        public string course_thumbnail { get; set; }

        public decimal price { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime updated_at { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_chapters> course_chapters { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_enrollments> course_enrollments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_reviews> course_reviews { get; set; }

        public virtual user user { get; set; }

        public virtual topic topic { get; set; }
    }
}
