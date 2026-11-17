using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        private Customer CurrentCustomer()
        {
            string username =
                Convert.ToString(Session["Username"]);

            string role =
                Convert.ToString(Session["UserRole"]).Trim();

            if (string.IsNullOrWhiteSpace(username) || role != "C")
            {
                return null;
            }

            return db.Customers.FirstOrDefault(
                c => c.Username == username &&
                     c.User.UserRole.Trim() == "C");
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (string.IsNullOrWhiteSpace(
                Convert.ToString(Session["Username"])))
            {
                return RedirectToAction(
                    "Login", "Account", new { area = "" });
            }

            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return new HttpStatusCodeResult(
                    403, "Tài khoản không có hồ sơ khách hàng.");
            }

            return View(new CustomerProfileViewModel
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                CustomerEmail = customer.CustomerEmail,
                CustomerAddress = customer.CustomerAddress
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CustomerProfileViewModel model)
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return new HttpStatusCodeResult(403);
            }

            // Luôn lấy tài khoản từ phiên đăng nhập.
            model.Username = customer.Username;
            ModelState.Remove("Username");

            byte[] avatarBytes = null;

            if (model.Avatar != null &&
                model.Avatar.ContentLength > 0)
            {
                if (model.Avatar.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Avatar", "Ảnh tối đa 2 MB.");
                }
                else
                {
                    try
                    {
                        avatarBytes = ConvertAvatar(model.Avatar);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);

                        ModelState.AddModelError(
                            "Avatar",
                            "Ảnh không hợp lệ. Chọn ảnh JPG hoặc PNG.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (avatarBytes != null)
                {
                    string path = AvatarPath(customer.Username);

                    Directory.CreateDirectory(
                        Path.GetDirectoryName(path));

                    System.IO.File.WriteAllBytes(path, avatarBytes);
                }

                customer.CustomerName = model.CustomerName.Trim();
                customer.CustomerPhone = model.CustomerPhone.Trim();

                customer.CustomerEmail =
                    (model.CustomerEmail ?? "").Trim();

                customer.CustomerAddress =
                    (model.CustomerAddress ?? "").Trim();

                db.SaveChanges();

                Session["CustomerName"] = customer.CustomerName;

                TempData["ProfileSuccess"] =
                    "Cập nhật thông tin thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Chưa lưu được đầy đủ thay đổi. Vui lòng thử lại.");

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Avatar()
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return new HttpStatusCodeResult(403);
            }

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            string path = AvatarPath(customer.Username);

            if (!System.IO.File.Exists(path))
            {
                return HttpNotFound();
            }

            return File(
                System.IO.File.ReadAllBytes(path),
                "image/png");
        }

        private string AvatarPath(string username)
        {
            using (var sha = SHA256.Create())
            {
                string filename = BitConverter.ToString(
                    sha.ComputeHash(
                        Encoding.UTF8.GetBytes(username)))
                    .Replace("-", "");

                return Path.Combine(
                    Server.MapPath("~/App_Data/CustomerAvatars"),
                    filename + ".png");
            }
        }

        private byte[] ConvertAvatar(HttpPostedFileBase file)
        {
            using (var source = Image.FromStream(
                file.InputStream, true, true))
            {
                bool valid =
                    source.RawFormat.Guid == ImageFormat.Jpeg.Guid ||
                    source.RawFormat.Guid == ImageFormat.Png.Guid;

                if (!valid ||
                    source.Width > 5000 ||
                    source.Height > 5000 ||
                    (long)source.Width * source.Height > 16000000)
                {
                    throw new ArgumentException("Invalid image");
                }

                double scale = Math.Min(
                    1.0, 400.0 / Math.Max(source.Width, source.Height));

                int width = Math.Max(1, (int)(source.Width * scale));
                int height = Math.Max(1, (int)(source.Height * scale));

                using (var resized = new Bitmap(width, height))
                {
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.Clear(Color.White);
                        graphics.InterpolationMode =
                            InterpolationMode.HighQualityBicubic;

                        graphics.DrawImage(
                            source, 0, 0, width, height);
                    }

                    using (var buffer = new MemoryStream())
                    {
                        resized.Save(buffer, ImageFormat.Png);
                        return buffer.ToArray();
                    }
                }
            }
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