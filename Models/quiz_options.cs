namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class quiz_options
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public quiz_options()
        {
            quiz_attempt_answers = new HashSet<quiz_attempt_answers>();
        }

        public long id { get; set; }

        public long question_id { get; set; }

        [Required]
        [StringLength(255)]
        public string option_text { get; set; }

        public bool is_correct { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<quiz_attempt_answers> quiz_attempt_answers { get; set; }

        public virtual quiz_questions quiz_questions { get; set; }
    }
}
