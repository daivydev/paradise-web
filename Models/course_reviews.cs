namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class course_reviews
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        public long? course_id { get; set; }

        public long? user_id { get; set; }

        public int rating { get; set; }

        [StringLength(1)]
        public string review_text { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public bool is_visible { get; set; }

        public virtual cours cours { get; set; }

        public virtual user user { get; set; }
    }
}
