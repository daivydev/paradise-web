namespace paradise.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class lesson_contents
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long id { get; set; }

        public long? lesson_id { get; set; }

        [Required]
        [StringLength(10)]
        public string content_type { get; set; }

        public string content_text { get; set; }

        public string content_url { get; set; }

        public int display_order { get; set; }


        [Column(TypeName = "datetime2")]
        public DateTime created_at { get; set; }
        public bool is_visible { get; set; } = true;


        public virtual course_lessons course_lessons { get; set; }
    }
}
