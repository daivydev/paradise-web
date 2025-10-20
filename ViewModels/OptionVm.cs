using System.Web.Mvc;

namespace paradise.ViewModels
{
    public class OptionVm
    {
        public long? id { get; set; }

        [AllowHtml]
        public string option_text { get; set; }

        public bool _remove { get; set; }
    }
}
