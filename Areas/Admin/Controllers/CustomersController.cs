using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class CustomersController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index(string searchTerm)
        {
            searchTerm = (searchTerm ?? "").Trim();

            var customers = db.Customers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                customers = customers.Where(c =>
                    c.CustomerName.Contains(searchTerm) ||
                    c.CustomerPhone.Contains(searchTerm) ||
                    c.CustomerEmail.Contains(searchTerm) ||
                    c.Username.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;

            return View(customers
                .OrderByDescending(c => c.CustomerID)
                .ToList());
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(400);
            }

            Customer customer = db.Customers
                .AsNoTracking()
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.CustomerID == id.Value);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
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