using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace paradise.Models.ViewModels
{
    public class CourseListVM
    {
        public IEnumerable<CourseListItemVM> Items { get; set; }

        public string Q { get; set; }
        public int? TopicId { get; set; }

        public int Page { get; set; }
        public int Total { get; set; }

        public IEnumerable<SelectListItem> Topics { get; set; }
    }
}