namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class lesson_progress
    {
        public long id { get; set; }

        public long user_id { get; set; }

        public long lesson_id { get; set; }

        public bool is_completed { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? completed_at { get; set; }

        public virtual course_lessons course_lessons { get; set; }

        public virtual user user { get; set; }
    }
}
