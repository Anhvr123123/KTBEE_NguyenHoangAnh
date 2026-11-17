using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class CategoriesController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var categories = db.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                categories = categories.Where(
                    c => c.CategoryName.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(categories
                .OrderBy(c => c.CategoryName)
                .ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryName,Description")]
            Category category)
        {
            category.CategoryName =
                (category.CategoryName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Vui lòng nhập tên hãng xe.");
            }

            if (db.Categories.Any(
                c => c.CategoryName == category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Tên hãng xe đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                db.Categories.Add(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Thêm hãng xe thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể thêm hãng xe. Vui lòng thử lại.");

                return View(category);
            }
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(400);
            }

            Category category = db.Categories.Find(id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CategoryID,CategoryName,Description")]
            Category category)
        {
            Category existing = db.Categories.Find(
                category.CategoryID);

            if (existing == null)
            {
                return HttpNotFound();
            }

            category.CategoryName =
                (category.CategoryName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Vui lòng nhập tên hãng xe.");
            }

            if (db.Categories.Any(c =>
                c.CategoryName == category.CategoryName &&
                c.CategoryID != category.CategoryID))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Tên hãng xe đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;

                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật hãng xe thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật hãng xe. Vui lòng thử lại.");

                return View(category);
            }
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(400);
            }

            Category category = db.Categories.Find(id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            if (db.Products.Any(p => p.CategoryID == id))
            {
                TempData["Error"] =
                    "Không thể xóa hãng đang có sản phẩm.";

                return RedirectToAction("Index");
            }

            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();

                TempData["Success"] =
                    "Xóa hãng xe thành công.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                TempData["Error"] =
                    "Không thể xóa hãng xe vì còn dữ liệu liên quan.";
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