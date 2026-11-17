using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class AdminUsersController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var users = db.Users.Where(
                u => u.UserRole.Trim() == "A");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(
                    u => u.Username.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            // Chỉ gửi tên đăng nhập sang View.
            // Không gửi mật khẩu hoặc chuỗi hash.
            var model = users
                .OrderBy(u => u.Username)
                .Select(u => new AdminUserViewModel
                {
                    Username = u.Username
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.IsCreate = true;

            return View("Form", new AdminUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminUserViewModel model)
        {
            ViewBag.IsCreate = true;

            model.Username = (model.Username ?? "").Trim();

            if (model.Username.Length < 4)
            {
                ModelState.AddModelError(
                    "Username",
                    "Tên đăng nhập phải có ít nhất 4 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Vui lòng nhập mật khẩu.");
            }

            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError(
                    "ConfirmPassword",
                    "Vui lòng nhập lại mật khẩu.");
            }

            if (db.Users.Any(
                u => u.Username == model.Username))
            {
                ModelState.AddModelError(
                    "Username",
                    "Tên đăng nhập đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                db.Users.Add(new User
                {
                    Username = model.Username,
                    Password = PasswordHelper.HashPassword(
                        model.Password),
                    UserRole = "A"
                });

                db.SaveChanges();

                TempData["Success"] =
                    "Thêm tài khoản Admin thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể tạo tài khoản. Vui lòng thử lại.");

                return View("Form", model);
            }
        }

        [HttpGet]
        public ActionResult Edit(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(400);
            }

            User user = db.Users.FirstOrDefault(
                u => u.Username == username &&
                     u.UserRole.Trim() == "A");

            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.IsCreate = false;

            return View("Form", new AdminUserViewModel
            {
                Username = user.Username
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            string originalUsername,
            AdminUserViewModel model)
        {
            ViewBag.IsCreate = false;

            if (string.IsNullOrWhiteSpace(originalUsername))
            {
                return new HttpStatusCodeResult(400);
            }

            User user = db.Users.FirstOrDefault(
                u => u.Username == originalUsername &&
                     u.UserRole.Trim() == "A");

            if (user == null)
            {
                return HttpNotFound();
            }

            // Không cho sửa tên đăng nhập là khóa tài khoản.
            model.Username = user.Username;
            ModelState.Remove("Username");

            bool hasPassword =
                !string.IsNullOrEmpty(model.Password);

            bool hasConfirmation =
                !string.IsNullOrEmpty(model.ConfirmPassword);

            if (hasPassword || hasConfirmation)
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(
                        "Password",
                        "Vui lòng nhập mật khẩu mới.");
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        "Vui lòng nhập lại mật khẩu mới.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            if (!hasPassword && !hasConfirmation)
            {
                TempData["Success"] =
                    "Không có thay đổi. Mật khẩu được giữ nguyên.";

                return RedirectToAction("Index");
            }

            try
            {
                user.Password = PasswordHelper.HashPassword(
                    model.Password);

                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật mật khẩu Admin thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật tài khoản. Vui lòng thử lại.");

                return View("Form", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpStatusCodeResult(400);
            }

            using (var transaction = db.Database.BeginTransaction(
                IsolationLevel.Serializable))
            {
                try
                {
                    User user = db.Users.FirstOrDefault(
                        u => u.Username == username &&
                             u.UserRole.Trim() == "A");

                    if (user == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    string currentUsername =
                        Convert.ToString(Session["Username"]);

                    if (string.Equals(
                        user.Username,
                        currentUsername,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Không thể xóa tài khoản đang đăng nhập.";

                        return RedirectToAction("Index");
                    }

                    int adminCount = db.Users.Count(
                        u => u.UserRole.Trim() == "A");

                    if (adminCount <= 1)
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Không thể xóa Admin cuối cùng.";

                        return RedirectToAction("Index");
                    }

                    bool hasCustomer = db.Customers.Any(
                        c => c.Username == user.Username);

                    if (hasCustomer)
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Tài khoản có hồ sơ khách hàng liên quan, " +
                            "không thể xóa.";

                        return RedirectToAction("Index");
                    }

                    db.Users.Remove(user);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["Success"] =
                        "Xóa tài khoản Admin thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    TempData["Error"] =
                        "Không thể xóa tài khoản vì còn dữ liệu " +
                        "liên quan hoặc có lỗi khi lưu.";
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