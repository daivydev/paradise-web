using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using paradise.Models;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminUsersController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Admin/AdminUsers
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

            var query = db.users.Include(u => u.user_profiles).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.email.Contains(search) ||
                    u.user_profiles.first_name.Contains(search) ||
                    u.user_profiles.last_name.Contains(search) ||
                    (u.user_profiles.first_name + " " + u.user_profiles.last_name).Contains(search)
                );
            }

            // Áp dụng phân trang trên query đã lọc
            var users = query
                .OrderBy(u => u.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalUsers = query.Count(); // Đếm tổng sau khi search

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            ViewBag.Search = search;

            return View(users);
        }

        // GET: Admin/AdminUsers/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/AdminUsers/Create
        public ActionResult Create()
        {
            ViewBag.roles = new SelectList(db.roles.ToList(), "id", "role_name");
            return View();
        }

        // POST: Admin/AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(user user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Khởi tạo user_profiles nếu null
                    if (user.user_profiles == null)
                    {
                        user.user_profiles = new user_profiles();
                    }

                    if (db.users.Any(u => u.email == user.email))
                    {
                        ModelState.AddModelError("CustomError", "Email này đã tồn tại!");
                    }

                    if (ModelState.IsValid) // Chỉ lưu khi không có lỗi
                    {
                        // Set thời gian
                        user.created_at = DateTime.Now;
                        user.updated_at = DateTime.Now;
                        user.user_profiles.created_at = DateTime.Now;
                        user.user_profiles.updated_at = DateTime.Now;

                        db.users.Add(user);
                        db.SaveChanges();

                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi & báo ra giao diện
                    ModelState.AddModelError("CustomError", "Có lỗi xảy ra khi tạo người dùng: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại SelectList + return view
            ViewBag.Roles = new SelectList(db.roles.ToList(), "id", "role_name", user.user_profiles?.role_id);
            return View(user);
        }

        // GET: Admin/AdminUsers/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = new SelectList(db.user_profiles, "user_id", "first_name", user.id);
            ViewBag.roles = new SelectList(db.roles.ToList(), "id", "role_name");
            return View(user);
        }

        // POST: Admin/AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(user user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm user hiện tại trong DB
                    var existingUser = db.users.Include("user_profiles").FirstOrDefault(u => u.id == user.id);
                    if (existingUser == null)
                    {
                        return HttpNotFound();
                    }

                    // Kiểm tra email trùng với user khác
                    if (db.users.Any(u => u.email == user.email && u.id != user.id))
                    {
                        ModelState.AddModelError("CustomError", "Email này đã tồn tại!");
                    }

                    if (ModelState.IsValid)
                    {
                        // Cập nhật các trường cơ bản
                        existingUser.email = user.email;
                        // Nếu password được nhập mới thì mới cập nhật
                        if (!string.IsNullOrEmpty(user.password))
                        {
                            existingUser.password = user.password;
                        }
                        existingUser.updated_at = DateTime.Now;

                        // Cập nhật user_profiles
                        existingUser.user_profiles.first_name = user.user_profiles.first_name;
                        existingUser.user_profiles.last_name = user.user_profiles?.last_name;
                        existingUser.user_profiles.role_id = user.user_profiles.role_id;
                        existingUser.user_profiles.gender = user.user_profiles.gender;
                        existingUser.user_profiles.date_of_birth = user.user_profiles.date_of_birth;
                        existingUser.user_profiles.updated_at = DateTime.Now;

                        db.SaveChanges();
                        ModelState.Clear();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CustomError", "Có lỗi xảy ra khi cập nhật người dùng: " + ex.Message);
                }
            }

            // Load lại SelectList khi có lỗi
            ViewBag.roles = new SelectList(db.roles.ToList(), "id", "role_name", user.user_profiles?.role_id);
            return View(user);
        }

        // GET: Admin/AdminUser/Delete/5  (chỉ để match route, không cần view vì dùng Popup Modal)
        public ActionResult Delete(long id)
        {
            var user = db.users.Find(id);
            if (user == null) return HttpNotFound();
            return RedirectToAction("Index");
        }

        // POST: Admin/AdminUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var user = db.users.Find(id);
            if (user != null)
            {
                db.users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        // Hàm này đảm bảo rằng mỗi request xong thì DbContext cũng được giải phóng sạch, tránh rò rỉ tài nguyên.
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
