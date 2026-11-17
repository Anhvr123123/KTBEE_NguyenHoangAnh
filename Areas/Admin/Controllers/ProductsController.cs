
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;
using KTBEE_NguyenHoangAnh.Filters;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class ProductsController : Controller
    {
        private LuxuryAutoEntities db = new LuxuryAutoEntities();

        public ActionResult Index(string searchTerm)
        {
            var products = db.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(products
                .OrderByDescending(p => p.ProductID)
                .ToList());
        }

        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            Product product,
            HttpPostedFileBase UploadImage,
            int quantity = 0)
        {
            ValidateProduct(product, quantity);

            if (UploadImage != null &&
                UploadImage.ContentLength > 0 &&
                !IsValidImage(UploadImage))
            {
                ModelState.AddModelError(
                    "UploadImage",
                    "Ảnh không hợp lệ. Chỉ chấp nhận JPG, PNG, WEBP tối đa 5MB.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(product.CategoryID);
                ViewBag.Quantity = quantity;
                return View(product);
            }

            if (UploadImage != null &&
                UploadImage.ContentLength > 0)
            {
                product.ProductImage = SaveImage(UploadImage);
            }

            db.Products.Add(product);
            db.SaveChanges();

            ProductDetail detail = new ProductDetail
            {
                ProductID = product.ProductID,
                Quantity = quantity,
                QuantitySold = 0
            };

            db.ProductDetails.Add(detail);
            db.SaveChanges();

            TempData["Success"] = "Thêm xe thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            LoadCategories(product.CategoryID);

            ViewBag.Quantity = product.ProductDetails
                .Select(d => d.Quantity)
                .FirstOrDefault();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            Product product,
            HttpPostedFileBase UploadImage,
            int quantity = 0)
        {
            Product existing = db.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefault(
                    p => p.ProductID == product.ProductID);

            if (existing == null)
            {
                return HttpNotFound();
            }

            ValidateProduct(product, quantity);

            if (UploadImage != null &&
                UploadImage.ContentLength > 0 &&
                !IsValidImage(UploadImage))
            {
                ModelState.AddModelError(
                    "UploadImage",
                    "Ảnh không hợp lệ. Chỉ chấp nhận JPG, PNG, WEBP tối đa 5MB.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(product.CategoryID);
                ViewBag.Quantity = quantity;
                return View(product);
            }

            existing.ProductName = product.ProductName;
            existing.CategoryID = product.CategoryID;
            existing.ProductPrice = product.ProductPrice;
            existing.ProductDescription = product.ProductDescription;
            existing.ModelYear = product.ModelYear;
            existing.CarColor = product.CarColor;
            existing.Engine = product.Engine;
            existing.Transmission = product.Transmission;

            if (UploadImage != null &&
                UploadImage.ContentLength > 0)
            {
                existing.ProductImage = SaveImage(UploadImage);
            }
            else if (!string.IsNullOrWhiteSpace(product.ProductImage))
            {
                existing.ProductImage = product.ProductImage.Trim();
            }

            ProductDetail detail =
                existing.ProductDetails.FirstOrDefault();

            if (detail == null)
            {
                detail = new ProductDetail
                {
                    ProductID = existing.ProductID,
                    Quantity = quantity,
                    QuantitySold = 0
                };

                db.ProductDetails.Add(detail);
            }
            else
            {
                detail.Quantity = quantity;
            }

            db.SaveChanges();

            TempData["Success"] =
                "Cập nhật sản phẩm thành công!";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return HttpNotFound();

            if (db.OrderDetails.Any(d => d.ProductID == id))
            {
                TempData["Error"] =
                    "Không thể xóa xe đã có trong đơn hàng.";

                return RedirectToAction("Index");
            }

            foreach (var detail in product.ProductDetails.ToList())
            {
                db.ProductDetails.Remove(detail);
            }

            db.Products.Remove(product);
            db.SaveChanges();

            TempData["Success"] = "Xóa xe thành công!";

            return RedirectToAction("Index");
        }

        private void LoadCategories(object selected = null)
        {
            ViewBag.CategoryID = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName),
                "CategoryID",
                "CategoryName",
                selected);
        }

        private void ValidateProduct(Product product, int quantity)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Vui lòng nhập tên xe.");
            }

            if (product.ProductPrice <= 0)
            {
                ModelState.AddModelError(
                    "ProductPrice",
                    "Giá bán phải lớn hơn 0.");
            }

            if (!db.Categories.Any(
                c => c.CategoryID == product.CategoryID))
            {
                ModelState.AddModelError(
                    "CategoryID",
                    "Vui lòng chọn hãng xe hợp lệ.");
            }

            if (quantity < 0)
            {
                ModelState.AddModelError(
                    "quantity",
                    "Số lượng không được âm.");
            }
        }

        private bool IsValidImage(HttpPostedFileBase file)
        {
            string extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            string[] allowed =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            return allowed.Contains(extension)
                && file.ContentLength <= 5 * 1024 * 1024;
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath("~/Images/cars/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            string fileName =
                Guid.NewGuid().ToString("N") + extension;

            string path = Path.Combine(folder, fileName);

            file.SaveAs(path);

            return "~/Images/cars/" + fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
