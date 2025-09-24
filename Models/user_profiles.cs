namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class user_profiles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long user_id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [StringLength(10, ErrorMessage = "Tên tối đa 10 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên không được chứa ký tự đặc biệt hoặc số")]
        public string first_name { get; set; }

        [StringLength(30, ErrorMessage = "Họ tối đa 30 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ không được chứa ký tự đặc biệt hoặc số")]
        public string last_name { get; set; }

        [StringLength(10, ErrorMessage = "Số điện thoại tối đa 10 ký tự")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số điện thoại chỉ được chứa chữ số")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string phone_number { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không hợp lệ")]
        public DateTime? date_of_birth { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [StringLength(10)]
        public string gender { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn role")]
        public long role_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime updated_at { get; set; } = DateTime.Now;

        [ForeignKey("role_id")]
        public virtual role role { get; set; }

        public virtual user user { get; set; }
    }
}
