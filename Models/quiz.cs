using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace paradise.Models
{
    [Table("quiz")]
    public partial class quiz
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz()
        {
            quiz_attempts = new HashSet<quiz_attempts>();
            quiz_questions = new HashSet<quiz_questions>();
        }

        // 🔹 IDENTITY tự tăng
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã Quiz")]
        public long id { get; set; }

        // 🔹 Tiêu đề Quiz
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề quiz.")]
        [StringLength(255, ErrorMessage = "Tiêu đề tối đa 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string title { get; set; }

        // 🔹 Chủ đề (FK)
        [Required(ErrorMessage = "Vui lòng chọn chủ đề.")]
        [Range(1, long.MaxValue, ErrorMessage = "Chủ đề không hợp lệ.")]
        [Display(Name = "Chủ đề")]
        public long topic { get; set; }

        // 🔹 Thời lượng (nullable)
        [Range(0, 9999, ErrorMessage = "Thời lượng phải ≥ 0 phút.")]
        [Display(Name = "Thời lượng (phút)")]
        public decimal? time { get; set; }

        // 🔹 Có giới hạn thời gian hay không
        [Display(Name = "Không giới hạn thời gian")]
        public bool is_infinity { get; set; }

        // 🔹 Số câu hỏi trong quiz
        [Required(ErrorMessage = "Vui lòng nhập số câu hỏi.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quiz phải có ít nhất 1 câu hỏi.")]
        [Display(Name = "Số câu hỏi")]
        public int quantity { get; set; }

        // 🔹 Thời gian tạo
        [Display(Name = "Ngày tạo")]
        public DateTime? created_at { get; set; }

        // 🔹 Cập nhật lần cuối
        [Display(Name = "Ngày cập nhật")]
        public DateTime? updated_at { get; set; }

        // ================== NAVIGATION PROPERTIES ==================
        [ForeignKey("topic")]
        [Display(Name = "Chủ đề Quiz")]
        public virtual topic_quiz topic_quiz { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempts> quiz_attempts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_questions> quiz_questions { get; set; }
    }
}
