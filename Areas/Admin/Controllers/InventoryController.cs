using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class InventoryController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var query = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            var products = query
                .OrderBy(p => p.ProductName)
                .ToList();

            var rows = new List<InventoryRowViewModel>();

            foreach (var product in products)
            {
                var stock = product.ProductDetails
                    .OrderBy(d => d.ProductDetailID)
                    .FirstOrDefault();

                rows.Add(new InventoryRowViewModel
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,

                    CategoryName = product.Category != null
                        ? product.Category.CategoryName
                        : "Chưa phân loại",

                    Quantity = stock == null ? 0 : stock.Quantity,

                    QuantitySold = stock == null
                        ? 0
                        : stock.QuantitySold
                });
            }

            return View(new InventoryViewModel
            {
                SearchTerm = searchTerm,

                InStock = rows
                    .Where(r => r.Available > 0)
                    .ToList(),

                NeedImport = rows
                    .Where(r => r.Available <= 0)
                    .ToList()
            });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Product product = db.Products
                .AsNoTracking()
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            ProductDetail stock = product.ProductDetails
                .OrderBy(d => d.ProductDetailID)
                .FirstOrDefault();

            return View(new InventoryEditViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Quantity = stock == null ? 0 : stock.Quantity,
                QuantitySold = stock == null ? 0 : stock.QuantitySold
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InventoryEditViewModel model)
        {
            using (var transaction = db.Database.BeginTransaction(
                IsolationLevel.Serializable))
            {
                try
                {
                    Product product = db.Products
                        .FirstOrDefault(
                            p => p.ProductID == model.ProductID);

                    if (product == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    ProductDetail stock = db.ProductDetails
                        .Where(d => d.ProductID == product.ProductID)
                        .OrderBy(d => d.ProductDetailID)
                        .FirstOrDefault();

                    int sold = stock == null
                        ? 0
                        : stock.QuantitySold;

                    // Thông tin này luôn lấy lại từ database.
                    model.ProductName = product.ProductName;
                    model.QuantitySold = sold;

                    ModelState.Remove("ProductName");
                    ModelState.Remove("QuantitySold");

                    if (model.Quantity.HasValue &&
                        model.Quantity.Value < sold)
                    {
                        ModelState.AddModelError(
                            "Quantity",
                            "Tổng số lượng không được nhỏ hơn " +
                            sold + " xe đã bán/đã giữ cho đơn hàng.");
                    }

                    if (!ModelState.IsValid)
                    {
                        transaction.Rollback();
                        return View(model);
                    }

                    if (stock == null)
                    {
                        stock = new ProductDetail
                        {
                            ProductID = product.ProductID,
                            Quantity = model.Quantity.Value,
                            QuantitySold = 0
                        };

                        db.ProductDetails.Add(stock);
                    }
                    else
                    {
                        stock.Quantity = model.Quantity.Value;
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] =
                        "Cập nhật kho thành công.";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật kho. Vui lòng thử lại.");

                    return View(model);
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