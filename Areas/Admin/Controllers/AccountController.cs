using System;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Login()
        {
            if (IsAdmin())
            {
                return RedirectToAction(
                    "Index", "Admin", new { area = "Admin" });
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = (model.Username ?? "").Trim();

            User user = db.Users.FirstOrDefault(
                u => u.Username == username);

            if (user == null ||
                (user.UserRole ?? "").Trim() != "A" ||
                !PasswordHelper.VerifyPassword(
                    model.Password, user.Password))
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu Admin không đúng.");

                return View(model);
            }

            Session.Clear();

            Session["Username"] = user.Username;
            Session["UserRole"] = "A";

            return RedirectToAction(
                "Index", "Admin", new { area = "Admin" });
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (!IsAdmin())
            {
                return RedirectToAction(
                    "Login", "Account", new { area = "Admin" });
            }

            return View(new AdminRegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AdminRegisterViewModel model)
        {
            if (!IsAdmin())
            {
                return new HttpStatusCodeResult(403);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = (model.Username ?? "").Trim();

            if (db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError(
                    "Username",
                    "Tên đăng nhập đã tồn tại.");

                return View(model);
            }

            try
            {
                User admin = new User
                {
                    Username = username,
                    Password = PasswordHelper.HashPassword(
                        model.Password),
                    UserRole = "A"
                };

                db.Users.Add(admin);
                db.SaveChanges();

                TempData["Success"] =
                    "Tạo tài khoản Admin mới thành công.";

                return RedirectToAction(
                    "Index", "Admin", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể tạo tài khoản Admin. Vui lòng thử lại.");

                return View(model);
            }
        }

        // Đăng xuất Admin rồi chuyển về trang khách hàng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GoToCustomer()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Index", "Home", new { area = "" });
        }

        // Đăng xuất rồi quay lại trang đăng nhập Admin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Login", "Account", new { area = "Admin" });
        }

        private bool IsAdmin()
        {
            string username =
                Convert.ToString(Session["Username"]);

            string role =
                Convert.ToString(Session["UserRole"]).Trim();

            return !string.IsNullOrWhiteSpace(username)
                   && role == "A";
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