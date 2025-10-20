namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("quiz_attempts")]
    public partial class quiz_attempts
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz_attempts()
        {
            quiz_attempt_answers = new HashSet<quiz_attempt_answers>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        [ForeignKey("user")]
        public long user_id { get; set; }

        [ForeignKey("quiz")]
        public long quiz_id { get; set; }

        public DateTime? started_at { get; set; }

        public DateTime? finished_at { get; set; }

        public int? score { get; set; }

        public virtual quiz quiz { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempt_answers> quiz_attempt_answers { get; set; }

        public virtual user user { get; set; }
    }
}
