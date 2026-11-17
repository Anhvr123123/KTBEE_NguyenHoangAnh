
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;
using KTBEE_NguyenHoangAnh.Filters;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class OrdersController : Controller
    {
        private LuxuryAutoEntities db = new LuxuryAutoEntities();

        public ActionResult Index(string searchTerm, string status)
        {
            var orders = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                int orderId;
                if (int.TryParse(searchTerm, out orderId))
                {
                    orders = orders.Where(o =>
                        o.OrderID == orderId ||
                        o.Customer.CustomerName.Contains(searchTerm));
                }
                else
                {
                    orders = orders.Where(o =>
                        o.Customer.CustomerName.Contains(searchTerm));
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.OrderStatus == status);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedStatus = status;

            return View(orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Order order = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id.Value);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            using (var transaction = db.Database.BeginTransaction(
                IsolationLevel.Serializable))
            {
                try
                {
                    Order order = db.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault(o => o.OrderID == id);

                    if (order == null)
                    {
                        transaction.Rollback();
                        return HttpNotFound();
                    }

                    string current = order.OrderStatus;

                    bool valid =
                        (current == "Chờ thanh toán" &&
                            (status == "Đang xử lý" ||
                             status == "Đã hủy")) ||

                        (current == "Đang xử lý" &&
                            (status == "Đang vận chuyển" ||
                             status == "Đã hủy")) ||

                        (current == "Đang vận chuyển" &&
                            status == "Đã giao");

                    if (!valid)
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Không thể chuyển từ '" + current +
                            "' sang '" + status + "'.";

                        return RedirectToAction("Details",
                            new { id = id });
                    }

                    if (status == "Đã hủy")
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            ProductDetail stock = db.ProductDetails
                                .FirstOrDefault(p =>
                                    p.ProductID == detail.ProductID);

                            if (stock == null ||
                                stock.QuantitySold < detail.Quantity)
                            {
                                throw new InvalidOperationException(
                                    "Không thể hoàn tồn kho cho xe " +
                                    detail.ProductID);
                            }

                            stock.QuantitySold -= detail.Quantity;
                        }
                    }

                    order.OrderStatus = status;

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] =
                        "Cập nhật trạng thái thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    TempData["Error"] =
                        "Không thể cập nhật trạng thái đơn hàng.";
                }
            }

            return RedirectToAction("Details",
                new { id = id });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
