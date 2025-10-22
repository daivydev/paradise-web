using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace paradise.Models.ViewModels
{
    public class CourseListItemVM
    { 
            public int Id { get; set; }
            public int? TopicId { get; set; }
            public string Title { get; set; }
            public string Thumbnail { get; set; }
            public string ShortDescription { get; set; }
            public decimal? Price { get; set; }
    }
}