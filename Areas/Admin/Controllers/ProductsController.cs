using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class ProductsController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        // DANH SÁCH SẢN PHẨM
        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductDetails)
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

        // THÊM SẢN PHẨM
        [HttpGet]
        public ActionResult Create()
        {
            LoadCategories();
            ViewBag.Quantity = 0;

            return View(new Product());
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
                    "Chỉ chấp nhận JPG, JPEG, PNG, WEBP tối đa 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(product.CategoryID);
                ViewBag.Quantity = quantity;

                return View(product);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (UploadImage != null &&
                        UploadImage.ContentLength > 0)
                    {
                        product.ProductImage = SaveImage(UploadImage);
                    }

                    db.Products.Add(product);
                    db.SaveChanges();

                    db.ProductDetails.Add(new ProductDetail
                    {
                        ProductID = product.ProductID,
                        Quantity = quantity,
                        QuantitySold = 0
                    });

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Thêm xe thành công.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    ModelState.AddModelError(
                        "",
                        "Không thể thêm xe. Vui lòng kiểm tra và thử lại.");
                }
            }

            LoadCategories(product.CategoryID);
            ViewBag.Quantity = quantity;

            return View(product);
        }

        // CHI TIẾT SẢN PHẨM
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // SỬA SẢN PHẨM
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            ProductDetail stock = product.ProductDetails
                .OrderBy(d => d.ProductDetailID)
                .FirstOrDefault();

            LoadCategories(product.CategoryID);

            ViewBag.Quantity =
                stock == null ? 0 : stock.Quantity;

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            Product product,
            HttpPostedFileBase UploadImage,
            int quantity = 0)
        {
            using (var transaction = db.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    Product existing = db.Products
                        .Include(p => p.ProductDetails)
                        .FirstOrDefault(
                            p => p.ProductID == product.ProductID);

                    if (existing == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    ValidateProduct(product, quantity);

                    ProductDetail stock = existing.ProductDetails
                        .OrderBy(d => d.ProductDetailID)
                        .FirstOrDefault();

                    int soldQuantity =
                        stock == null ? 0 : stock.QuantitySold;

                    if (quantity < soldQuantity)
                    {
                        ModelState.AddModelError(
                            "quantity",
                            "Tổng số lượng không được nhỏ hơn " +
                            soldQuantity +
                            " xe đã bán/giữ cho đơn hàng.");
                    }

                    if (UploadImage != null &&
                        UploadImage.ContentLength > 0 &&
                        !IsValidImage(UploadImage))
                    {
                        ModelState.AddModelError(
                            "UploadImage",
                            "Chỉ chấp nhận JPG, JPEG, PNG, WEBP tối đa 5 MB.");
                    }

                    if (!ModelState.IsValid)
                    {
                        transaction.Rollback();

                        LoadCategories(product.CategoryID);
                        ViewBag.Quantity = quantity;

                        return View(product);
                    }

                    existing.ProductName =
                        product.ProductName.Trim();

                    existing.CategoryID = product.CategoryID;
                    existing.ProductPrice = product.ProductPrice;
                    existing.ProductDescription =
                        product.ProductDescription;

                    existing.ModelYear = product.ModelYear;
                    existing.CarColor = product.CarColor;
                    existing.Engine = product.Engine;
                    existing.Transmission = product.Transmission;

                    if (UploadImage != null &&
                        UploadImage.ContentLength > 0)
                    {
                        existing.ProductImage = SaveImage(UploadImage);
                    }
                    else if (!string.IsNullOrWhiteSpace(
                        product.ProductImage))
                    {
                        existing.ProductImage =
                            product.ProductImage.Trim();
                    }

                    if (stock == null)
                    {
                        db.ProductDetails.Add(new ProductDetail
                        {
                            ProductID = existing.ProductID,
                            Quantity = quantity,
                            QuantitySold = 0
                        });
                    }
                    else
                    {
                        stock.Quantity = quantity;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] =
                        "Cập nhật sản phẩm thành công.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
                }
            }

            LoadCategories(product.CategoryID);
            ViewBag.Quantity = quantity;

            return View(product);
        }

        // XÓA SẢN PHẨM
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            if (db.OrderDetails.Any(d => d.ProductID == id))
            {
                TempData["Error"] =
                    "Không thể xóa xe đã có trong đơn hàng.";

                return RedirectToAction("Index");
            }

            try
            {
                foreach (var detail in product.ProductDetails.ToList())
                {
                    db.ProductDetails.Remove(detail);
                }

                db.Products.Remove(product);
                db.SaveChanges();

                TempData["Success"] = "Xóa xe thành công.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                TempData["Error"] =
                    "Không thể xóa xe vì còn dữ liệu liên quan.";
            }

            return RedirectToAction("Index");
        }

        // DỮ LIỆU HÃNG XE CHO DROPDOWN
        private void LoadCategories(object selected = null)
        {
            ViewBag.CategoryID = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName),
                "CategoryID",
                "CategoryName",
                selected);
        }

        // KIỂM TRA DỮ LIỆU
        private void ValidateProduct(Product product, int quantity)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Vui lòng nhập tên xe.");
            }
            else if (product.ProductName.Trim().Length > 150)
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên xe tối đa 150 ký tự.");
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

            if (product.ModelYear.HasValue &&
                (product.ModelYear.Value < 1886 ||
                 product.ModelYear.Value > DateTime.Now.Year + 1))
            {
                ModelState.AddModelError(
                    "ModelYear",
                    "Năm sản xuất không hợp lệ.");
            }

            if (!string.IsNullOrEmpty(product.CarColor) &&
                product.CarColor.Length > 50)
            {
                ModelState.AddModelError(
                    "CarColor",
                    "Màu sắc tối đa 50 ký tự.");
            }

            if (!string.IsNullOrEmpty(product.Engine) &&
                product.Engine.Length > 100)
            {
                ModelState.AddModelError(
                    "Engine",
                    "Động cơ tối đa 100 ký tự.");
            }

            if (!string.IsNullOrEmpty(product.Transmission) &&
                product.Transmission.Length > 100)
            {
                ModelState.AddModelError(
                    "Transmission",
                    "Hộp số tối đa 100 ký tự.");
            }

            if (!string.IsNullOrEmpty(product.ProductImage) &&
                product.ProductImage.Length > 500)
            {
                ModelState.AddModelError(
                    "ProductImage",
                    "Đường dẫn ảnh tối đa 500 ký tự.");
            }
        }

        // KIỂM TRA FILE ẢNH
        private bool IsValidImage(HttpPostedFileBase file)
        {
            string extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            string[] allowed =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            return allowed.Contains(extension)
                   && file.ContentLength > 0
                   && file.ContentLength <= 5 * 1024 * 1024;
        }

        // LƯU FILE ẢNH
        private string SaveImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath("~/Images/cars/");

            Directory.CreateDirectory(folder);

            string extension = Path.GetExtension(
                file.FileName).ToLowerInvariant();

            string filename =
                Guid.NewGuid().ToString("N") + extension;

            file.SaveAs(Path.Combine(folder, filename));

            return "~/Images/cars/" + filename;
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