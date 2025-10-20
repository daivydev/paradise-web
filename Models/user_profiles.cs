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

        [Required]
        [StringLength(1)]
        public string first_name { get; set; }

        [StringLength(1)]
        public string last_name { get; set; }

        [Required]
        [StringLength(255)]
        public string phone_number { get; set; }

        [Column(TypeName = "date")]
        public DateTime date_of_birth { get; set; }

        [Required]
        [StringLength(10)]
        public string gender { get; set; }

        public long role_id { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public virtual role role { get; set; }

        public virtual user user { get; set; }
    }
}
