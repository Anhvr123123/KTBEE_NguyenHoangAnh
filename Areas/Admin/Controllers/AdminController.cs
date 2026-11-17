using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        public ActionResult Index()
        {
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalOrders = db.Orders.Count();

            return View();
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