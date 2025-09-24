namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class user
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public user()
        {
            course_enrollments = new HashSet<course_enrollments>();
            course_reviews = new HashSet<course_reviews>();
            courses = new HashSet<cours>();
            lesson_progress = new HashSet<lesson_progress>();
            quiz_attempts = new HashSet<quiz_attempts>();
        }
        [Key]
        public long id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [StringLength(255, ErrorMessage = "Email tối đa 255 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Email chỉ được chứa chữ cái, số và ký tự . _ % + - trước @")]
        public string email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(255, ErrorMessage = "Mật khẩu tối đa 255 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string password { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime updated_at { get; set; } = DateTime.Now;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_enrollments> course_enrollments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<course_reviews> course_reviews { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cours> courses { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lesson_progress> lesson_progress { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempts> quiz_attempts { get; set; }

        public virtual user_profiles user_profiles { get; set; }
    }
}
