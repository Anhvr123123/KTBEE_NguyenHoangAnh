using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class MyOrdersController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index()
        {
            string username =
                Convert.ToString(Session["Username"]);

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction(
                    "Login", "Account", new { area = "" });
            }

            string role =
                Convert.ToString(Session["UserRole"]).Trim();

            if (role != "C")
            {
                return new HttpStatusCodeResult(
                    403, "Vui lòng sử dụng tài khoản khách hàng.");
            }

            var customer = db.Customers.FirstOrDefault(
                c => c.Username == username &&
                     c.User.UserRole.Trim() == "C");

            if (customer == null)
            {
                return new HttpStatusCodeResult(
                    403, "Không tìm thấy hồ sơ khách hàng.");
            }


            var orders = db.Orders
                .AsNoTracking()
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            return View(orders);
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