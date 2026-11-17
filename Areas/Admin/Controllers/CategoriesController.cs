
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;
using KTBEE_NguyenHoangAnh.Filters;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class CategoriesController : Controller
    {
        private readonly LuxuryAutoEntities db = new LuxuryAutoEntities();
        // DANH SACH HANG XE
        public ActionResult Index(string searchTerm)
        {
            var categories = db.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                categories = categories.Where(c =>
                    c.CategoryName.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(categories.OrderBy(c => c.CategoryID).ToList());
        }

        // CHI TIET HANG XE
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            return View(category);
        }

        // THEM HANG XE - GET
        public ActionResult Create()
        {
            return View();
        }

        // THEM HANG XE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryName,Description")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName",
                    "Vui lòng nhập tên hãng xe.");
            }

            if (db.Categories.Any(c =>
                c.CategoryName == category.CategoryName))
            {
                ModelState.AddModelError("CategoryName",
                    "Tên hãng xe đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();

                TempData["Success"] = "Thêm hãng xe thành công!";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // SUA HANG XE - GET
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            return View(category);
        }

        // SUA HANG XE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CategoryID,CategoryName,Description")]
            Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName",
                    "Vui lòng nhập tên hãng xe.");
            }

            if (db.Categories.Any(c =>
                c.CategoryName == category.CategoryName &&
                c.CategoryID != category.CategoryID))
            {
                ModelState.AddModelError("CategoryName",
                    "Tên hãng xe đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                Category oldCategory =
                    db.Categories.Find(category.CategoryID);

                if (oldCategory == null)
                    return HttpNotFound();

                oldCategory.CategoryName = category.CategoryName;
                oldCategory.Description = category.Description;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật hãng xe thành công!";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // XOA HANG XE - GET
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            return View(category);
        }

        // XOA HANG XE - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
                return HttpNotFound();

            bool hasProducts = db.Products.Any(p =>
                p.CategoryID == id);

            if (hasProducts)
            {
                TempData["Error"] =
                    "Không thể xóa hãng xe vì đang có sản phẩm thuộc hãng này.";

                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Xóa hãng xe thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
