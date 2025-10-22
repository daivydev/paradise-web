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

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ đề")]

        public long topics_id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tác giả")]
        public long author_id { get; set; }


        [Required(ErrorMessage = "Vui lòng nhập tiêu đề khóa học")]
        [StringLength(255)]
        [Column(TypeName = "nvarchar")]  


        public string course_title { get; set; }

        [StringLength(1)]
        public string course_description { get; set; }

        [StringLength(255)]
        public string course_thumbnail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, Double.MaxValue, ErrorMessage = "Giá phải từ 1000 trở lên")]
        public decimal price { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public bool is_visible { get; set; } = true;


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
