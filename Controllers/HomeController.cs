using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class HomeController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        // TRANG CHỦ: CHỈ HIỆN XE CÒN HÀNG
        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var products = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.ProductDetails.Any(d =>
                    d.Quantity > d.QuantitySold));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(products
                .OrderByDescending(p => p.ProductID)
                .ToList());
        }

        // MENU DANH MỤC
        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToList();

            return PartialView(
                "~/Views/Shared/_CategoryMenu.cshtml",
                categories);
        }

        // TÌM KIẾM, LỌC VÀ PHÂN TRANG THEO HÃNG
        [HttpGet]
        public ActionResult Category(
            int id,
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            int? year,
            string sort = "newest",
            int page = 1)
        {
            const int pageSize = 6;

            Category category = db.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            searchTerm = (searchTerm ?? "").Trim();

            var categoryProducts = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p =>
                    p.CategoryID == id &&
                    p.ProductDetails.Any(d =>
                        d.Quantity > d.QuantitySold));

            var years = categoryProducts
                .Where(p => p.ModelYear.HasValue)
                .Select(p => p.ModelYear.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (minPrice.HasValue && minPrice.Value < 0)
            {
                ModelState.AddModelError(
                    "minPrice",
                    "Giá thấp nhất không được âm.");
            }

            if (maxPrice.HasValue && maxPrice.Value < 0)
            {
                ModelState.AddModelError(
                    "maxPrice",
                    "Giá cao nhất không được âm.");
            }

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                ModelState.AddModelError(
                    "",
                    "Giá thấp nhất phải nhỏ hơn hoặc bằng giá cao nhất.");
            }

            var products = categoryProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(
                    p => p.ProductName.Contains(searchTerm));
            }

            // Chỉ áp dụng bộ lọc giá/năm khi dữ liệu nhập hợp lệ.
            if (ModelState.IsValid)
            {
                if (minPrice.HasValue)
                {
                    products = products.Where(
                        p => p.ProductPrice >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    products = products.Where(
                        p => p.ProductPrice <= maxPrice.Value);
                }

                if (year.HasValue)
                {
                    products = products.Where(
                        p => p.ModelYear == year.Value);
                }
            }

            int totalItems = products.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize));

            page = Math.Max(1, Math.Min(page, totalPages));

            IOrderedQueryable<Product> ordered;

            switch (sort)
            {
                case "price_asc":
                    ordered = products
                        .OrderBy(p => p.ProductPrice)
                        .ThenBy(p => p.ProductID);
                    break;

                case "price_desc":
                    ordered = products
                        .OrderByDescending(p => p.ProductPrice)
                        .ThenByDescending(p => p.ProductID);
                    break;

                case "name":
                    ordered = products
                        .OrderBy(p => p.ProductName)
                        .ThenBy(p => p.ProductID);
                    break;

                default:
                    sort = "newest";

                    ordered = products
                        .OrderByDescending(p => p.ProductID);
                    break;
            }

            return View("Categories", new ProductCategoryViewModel
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,

                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Year = year,
                Sort = sort,

                Years = years,

                Items = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),

                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        // CHI TIẾT XE
        [HttpGet]
        public ActionResult Details(int id)
        {
            Product product = db.Products
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