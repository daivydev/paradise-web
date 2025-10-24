//using System;
//using System.IO;
//using System.Linq;
//using System.Web;
//using System.Xml.Linq;
//// using DocumentFormat.OpenXml.Packaging; // không cần nếu dùng WmlDocument
//using OpenXmlPowerTools;

//namespace paradise.Helpers
//{
//    public static class DocxToHtmlHelper
//    {
//        // Convert DOCX (đường dẫn web tương đối, ví dụ: /Uploads/LessonContents/abc.docx) -> HTML string
//        // Ảnh sẽ được trích ra lưu dưới /Uploads/HtmlCache/images/
//        public static string ConvertRelativeDocxToHtml(HttpServerUtilityBase server, string relPath)
//        {
//            if (string.IsNullOrWhiteSpace(relPath))
//                return "<p><em>Không tìm thấy file Word.</em></p>";

//            var srcAbs = server.MapPath(relPath);
//            if (!File.Exists(srcAbs))
//                return $"<p><em>Không tìm thấy file: {HttpUtility.HtmlEncode(relPath)}</em></p>";

//            // chỉ xử lý .docx
//            if (!string.Equals(Path.GetExtension(srcAbs), ".docx", StringComparison.OrdinalIgnoreCase))
//                return "<p><em>Tệp không phải .docx nên không thể chuyển sang HTML.</em></p>";

//            var cacheHtmlRel = "/Uploads/HtmlCache";
//            var cacheImgRel = "/Uploads/HtmlCache/images";
//            var cacheHtmlAbs = server.MapPath(cacheHtmlRel);
//            var cacheImgAbs = server.MapPath(cacheImgRel);
//            Directory.CreateDirectory(cacheHtmlAbs);
//            Directory.CreateDirectory(cacheImgAbs);

//            var outRel = cacheHtmlRel + "/" + Path.GetFileNameWithoutExtension(relPath) + ".html";
//            var outAbs = server.MapPath(outRel);

//            // Có cache thì dùng ngay
//            if (File.Exists(outAbs))
//                return File.ReadAllText(outAbs);

//            try
//            {
//                var settings = new HtmlConverterSettings
//                {
//                    ImageHandler = imageInfo =>
//                    {
//                        // tên file ảnh
//                        var ext = imageInfo.ContentType.Split('/').Last(); // png, jpeg, gif...
//                        var fileName = Guid.NewGuid().ToString("N") + "." + ext;
//                        var imgAbs = Path.Combine(cacheImgAbs, fileName);
//                        var imgRel = cacheImgRel + "/" + fileName;

//                        // lưu bitmap ra đĩa
//                        using (var fs = File.Create(imgAbs))
//                            imageInfo.Bitmap.Save(fs, imageInfo.Bitmap.RawFormat);

//                        // width/height: lấy từ Bitmap (KHÔNG dùng imageInfo.Image)
//                        var img = new XElement(Xhtml.img,
//                            new XAttribute(NoNamespace.src, imgRel),
//                            new XAttribute(NoNamespace.style,
//                                $"width:{imageInfo.Bitmap.Width}px;height:{imageInfo.Bitmap.Height}px;"));

//                        return img;
//                    }
//                };

//                // Đọc docx bằng WmlDocument (ổn định với HtmlConverter)
//                var wml = new WmlDocument(srcAbs);
//                var html = HtmlConverter.ConvertToHtml(wml, settings);

//                var htmlString = html.ToString(SaveOptions.DisableFormatting);
//                File.WriteAllText(outAbs, htmlString);
//                return htmlString;
//            }
//            catch (Exception ex)
//            {
//                return "<p><em>Không thể hiển thị file Word này: "
//                       + System.Web.HttpUtility.HtmlEncode(ex.Message) + "</em></p>";
//            }
//        }
//    }
//}
