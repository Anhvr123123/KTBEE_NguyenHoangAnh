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
            var customers = db.Customers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

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