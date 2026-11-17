
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class HomeController : Controller
    {
        private LuxuryAutoEntities db = new LuxuryAutoEntities();

        public ActionResult Index(string searchTerm)
        {
            var products = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(products
                .OrderByDescending(p => p.ProductID)
                .ToList());
        }

        public ActionResult Category(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryName = category.CategoryName;

            var products = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryID == id)
                .OrderByDescending(p => p.ProductID)
                .ToList();

            return View("Categories", products);
        }

        public ActionResult Details(int id)
        {
            var product = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductDetails)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
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
