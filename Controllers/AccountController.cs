using System;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class AccountController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        // ĐĂNG NHẬP TRANG KHÁCH HÀNG
        [HttpGet]
        public ActionResult Login()
        {
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
                !PasswordHelper.VerifyPassword(
                    model.Password, user.Password))
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            string role = (user.UserRole ?? "").Trim();

            if (role != "A" && role != "C")
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản không có quyền truy cập hợp lệ.");

                return View(model);
            }

            Session.Clear();

            Session["Username"] = user.Username;
            Session["UserRole"] = role;

            if (role == "C")
            {
                Customer customer = db.Customers.FirstOrDefault(
                    c => c.Username == user.Username);

                if (customer != null)
                {
                    Session["CustomerID"] = customer.CustomerID;
                    Session["CustomerName"] = customer.CustomerName;
                }
            }

            // Đăng nhập ở trang khách hàng thì về trang khách hàng.
            return RedirectToAction(
                "Index",
                "Home",
                new { area = "" });
        }

        // ĐĂNG KÝ KHÁCH HÀNG
        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
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

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    User user = new User
                    {
                        Username = username,
                        Password = PasswordHelper.HashPassword(
                            model.Password),
                        UserRole = "C"
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    Customer customer = new Customer
                    {
                        CustomerName =
                            (model.CustomerName ?? "").Trim(),

                        CustomerPhone =
                            (model.CustomerPhone ?? "").Trim(),

                        CustomerEmail = model.CustomerEmail,
                        CustomerAddress = model.CustomerAddress,
                        Username = username
                    };

                    db.Customers.Add(customer);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["Success"] =
                        "Đăng ký thành công. Vui lòng đăng nhập.";

                    return RedirectToAction(
                        "Login",
                        "Account",
                        new { area = "" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    ModelState.AddModelError(
                        "",
                        "Không thể đăng ký tài khoản. Vui lòng thử lại.");

                    return View(model);
                }
            }
        }

        // ĐĂNG KÝ ADMIN CÔNG KHAI
        [HttpGet]
        public ActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(
            string username,
            string password,
            string confirmPassword)
        {
            username = (username ?? "").Trim();

            if (username.Length < 4)
            {
                ViewBag.Error =
                    "Tên đăng nhập phải có ít nhất 4 ký tự.";

                return View();
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8)
            {
                ViewBag.Error =
                    "Mật khẩu phải có ít nhất 8 ký tự.";

                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error =
                    "Mật khẩu nhập lại không khớp.";

                return View();
            }

            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error =
                    "Tên đăng nhập đã tồn tại.";

                return View();
            }

            try
            {
                User admin = new User
                {
                    Username = username,
                    Password = PasswordHelper.HashPassword(password),
                    UserRole = "A"
                };

                db.Users.Add(admin);
                db.SaveChanges();

                TempData["Success"] =
                    "Tạo tài khoản Admin thành công. Vui lòng đăng nhập.";

                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Admin" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ViewBag.Error =
                    "Không thể tạo tài khoản Admin. Vui lòng thử lại.";

                return View();
            }
        }

        // ĐĂNG XUẤT TRANG KHÁCH HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Index",
                "Home",
                new { area = "" });
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