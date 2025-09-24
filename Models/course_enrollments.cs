namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class course_enrollments
    {
        public long id { get; set; }

        public long course_id { get; set; }

        public long user_id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime enrolled_at { get; set; }

        public virtual cours cours { get; set; }

        public virtual user user { get; set; }
    }
}
