using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using paradise.Models;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminCourse_EnrollmentsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin/AdminCourse_Enrollments
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                // Nếu search rỗng thì redirect về URL gốc không có query
                if (Request.QueryString["search"] != null)
                {
                    return RedirectToAction("Index");
                }
            }

            var query = db.course_enrollments.Include(e => e.cours).Include(e => e.user).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.cours.course_title.Contains(search) ||            // tìm theo tên khóa học
                    e.user.email.Contains(search)              // tìm theo email user
                );
            }

            var enrollments = query
            .OrderBy(e => e.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            int total = query.Count();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;


            return View(enrollments);
        }

        // GET: Admin/AdminCourse_Enrollments/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_enrollments course_enrollments = db.course_enrollments.Find(id);
            if (course_enrollments == null)
            {
                return HttpNotFound();
            }
            return View(course_enrollments);
        }

        // GET: Admin/AdminCourse_Enrollments/Create
        public ActionResult Create()
        {
            ViewBag.course_id = new SelectList(db.courses, "id", "course_title");
            ViewBag.user_id = new SelectList(db.users, "id", "email");
            return View();
        }

        // POST: Admin/AdminCourse_Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,course_id,user_id,enrolled_at")] course_enrollments course_enrollments)
        {
            // Kiểm tra duplicate
            bool exists = db.course_enrollments.Any(e => e.course_id == course_enrollments.course_id
                                                      && e.user_id == course_enrollments.user_id);
            if (exists)
            {
                ModelState.AddModelError("CustomError", "Người dùng đã đăng ký khóa học này!");
            }

            if (ModelState.IsValid)
            {
                db.course_enrollments.Add(course_enrollments);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.course_id = new SelectList(db.courses, "id", "course_title", course_enrollments.course_id);
            ViewBag.user_id = new SelectList(db.users, "id", "email", course_enrollments.user_id);
            return View(course_enrollments);
        }


        // GET: Admin/AdminCourse_Enrollments/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_enrollments course_enrollments = db.course_enrollments.Find(id);
            if (course_enrollments == null)
            {
                return HttpNotFound();
            }
            ViewBag.course_id = new SelectList(db.courses, "id", "course_title", course_enrollments.course_id);
            ViewBag.user_id = new SelectList(db.users, "id", "email", course_enrollments.user_id);
            return View(course_enrollments);
        }

        // POST: Admin/AdminCourse_Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,course_id,user_id,enrolled_at")] course_enrollments course_enrollments)
        {
            if (ModelState.IsValid)
            {
                db.Entry(course_enrollments).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.course_id = new SelectList(db.courses, "id", "course_title", course_enrollments.course_id);
            ViewBag.user_id = new SelectList(db.users, "id", "email", course_enrollments.user_id);
            return View(course_enrollments);
        }

        // GET: Admin/AdminCourse_Enrollments/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            course_enrollments course_enrollments = db.course_enrollments.Find(id);
            if (course_enrollments == null)
            {
                return HttpNotFound();
            }
            return View(course_enrollments);
        }

        // POST: Admin/AdminCourse_Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            course_enrollments course_enrollments = db.course_enrollments.Find(id);
            db.course_enrollments.Remove(course_enrollments);
            db.SaveChanges();
            return RedirectToAction("Index");
        }




        public string RemoveVietnameseSigns(string str)
        {
            string formD = str.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file, long course_id)
        {
            if (file == null || file.ContentLength == 0)
                return RedirectToAction("Index");

            var enrollmentsToAdd = new List<course_enrollments>();
            var usersToAdd = new List<user>();
            var random = new Random();

            using (var workbook = new XLWorkbook(file.InputStream))
            {
                var worksheet = workbook.Worksheets.First();
                int rowCount = worksheet.LastRowUsed().RowNumber();

                for (int row = 2; row <= rowCount; row++)
                {
                    string lastName = worksheet.Cell(row, 1).GetValue<string>().Trim();
                    string firstName = worksheet.Cell(row, 2).GetValue<string>().Trim();
                    string phone = worksheet.Cell(row, 3).GetValue<string>().Trim();
                    string gender = worksheet.Cell(row, 4).GetValue<string>().Trim();
                    string dobText = worksheet.Cell(row, 5).GetValue<string>().Trim();

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                        continue;

                    DateTime? dob = null;
                    if (DateTime.TryParse(dobText, out DateTime dobValue))
                        dob = dobValue;



                    // Tự sinh email
                    string cleanFirstName = RemoveVietnameseSigns(firstName).ToLower().Trim().Replace(" ", "").Replace("  ", " ");
                    string cleanLastName = RemoveVietnameseSigns(lastName).ToLower().Trim().Replace(" ", "").Replace("  ", " ");
                    string email = $"{cleanFirstName}{cleanLastName}{random.Next(1000, 9999)}@example.com".ToLower();

                    string genderDb = "Other"; // mặc định nếu không xác định

                    if (!string.IsNullOrEmpty(gender))
                    {
                        var g = gender.Trim().ToLower();
                        if (g == "male" || g == "nam") genderDb = "Male";
                        else if (g == "female" || g == "nữ") genderDb = "Female";
                        else genderDb = "Other";
                    }

                    // Kiểm tra user đã tồn tại chưa
                    var user = db.users.FirstOrDefault(u => u.email == email);
                    if (user == null)
                    {
                        user = new user
                        {
                            email = email,
                            password = cleanFirstName + cleanLastName + "@P123",
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now,
                            user_profiles = new user_profiles
                            {
                                first_name = firstName,
                                last_name = lastName,
                                phone_number = string.IsNullOrEmpty(phone) ? null : phone,
                                gender = genderDb,
                                date_of_birth = dob,
                                role_id = 2,
                                created_at = DateTime.Now,
                                updated_at = DateTime.Now
                            }
                        };

                        db.users.Add(user);
                        db.SaveChanges();
                    }

                    // Kiểm tra enroll đã tồn tại chưa
                    bool alreadyEnrolled = db.course_enrollments.Any(e => e.user_id == user.id && e.course_id == course_id);
                    if (!alreadyEnrolled)
                    {
                        enrollmentsToAdd.Add(new course_enrollments
                        {
                            user_id = user.id,
                            course_id = course_id,
                            enrolled_at = DateTime.Now
                        });
                    }
                }

                if (enrollmentsToAdd.Any())
                {
                    db.course_enrollments.AddRange(enrollmentsToAdd);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
