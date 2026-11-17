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
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class ProfileController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index()
        {
            string username = Convert.ToString(Session["Username"]);

            var user = db.Users.FirstOrDefault(
                u => u.Username == username);

            if (user == null ||
                (user.UserRole ?? "").Trim() != "A")
            {
                return new HttpStatusCodeResult(403);
            }

            ViewBag.Username = user.Username;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAvatar(HttpPostedFileBase avatar)
        {
            string username = Convert.ToString(Session["Username"]);

            bool isAdmin = db.Users.Any(
                u => u.Username == username &&
                     u.UserRole.Trim() == "A");

            if (!isAdmin)
            {
                return new HttpStatusCodeResult(403);
            }

            if (avatar == null || avatar.ContentLength <= 0)
            {
                TempData["ProfileError"] =
                    "Vui lòng chọn ảnh đại diện.";

                return RedirectToAction("Index");
            }

            if (avatar.ContentLength > 2 * 1024 * 1024)
            {
                TempData["ProfileError"] =
                    "Ảnh không được vượt quá 2 MB.";

                return RedirectToAction("Index");
            }

            string extension =
                Path.GetExtension(avatar.FileName).ToLowerInvariant();

            if (extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png")
            {
                TempData["ProfileError"] =
                    "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG.";

                return RedirectToAction("Index");
            }

            try
            {
                using (var source = Image.FromStream(
                    avatar.InputStream, true, true))
                {
                    bool validFormat =
                        source.RawFormat.Guid == ImageFormat.Jpeg.Guid ||
                        source.RawFormat.Guid == ImageFormat.Png.Guid;

                    if (!validFormat ||
                        source.Width > 5000 ||
                        source.Height > 5000 ||
                        (long)source.Width * source.Height > 16000000)
                    {
                        TempData["ProfileError"] =
                            "Ảnh không hợp lệ hoặc có kích thước quá lớn.";

                        return RedirectToAction("Index");
                    }
                    double scale = Math.Min(
                        1.0,
                        400.0 / Math.Max(source.Width, source.Height));

                    int width = Math.Max(
                        1, (int)(source.Width * scale));

                    int height = Math.Max(
                        1, (int)(source.Height * scale));

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

                        string path = GetAvatarPath(username);

                        Directory.CreateDirectory(
                            Path.GetDirectoryName(path));

                        using (var buffer = new MemoryStream())
                        {
                            resized.Save(buffer, ImageFormat.Png);

                            System.IO.File.WriteAllBytes(
                                path, buffer.ToArray());
                        }
                    }
                }

                TempData["Success"] =
                    "Cập nhật ảnh đại diện thành công.";
            }
            catch (ArgumentException)
            {
                TempData["ProfileError"] =
                    "Không thể đọc ảnh. Vui lòng chọn ảnh JPG hoặc PNG khác.";
            }
            catch (OutOfMemoryException)
            {
                TempData["ProfileError"] =
                    "Ảnh bị lỗi hoặc quá lớn. Vui lòng chọn ảnh khác.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                TempData["ProfileError"] =
                    "Chưa thể lưu ảnh đại diện. Vui lòng thử lại.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Avatar()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            string username = Convert.ToString(Session["Username"]);
            string path = GetAvatarPath(username);

            if (!System.IO.File.Exists(path))
            {
                return HttpNotFound();
            }

            return File(
                System.IO.File.ReadAllBytes(path),
                "image/png");
        }

        private string GetAvatarPath(string username)
        { 
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(username));

                string filename = BitConverter.ToString(hash)
                    .Replace("-", "")
                    .ToLowerInvariant();

                return Path.Combine(
                    Server.MapPath("~/App_Data/AdminAvatars"),
                    filename + ".png");
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