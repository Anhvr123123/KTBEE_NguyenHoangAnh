using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Filters;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Areas.Admin.Controllers
{
    [AdminOnly]
    public class OrdersController : Controller
    {
        private readonly LuxuryAutoEntities db =
            new LuxuryAutoEntities();

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string status,
            string paymentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            string sort = "newest",
            int page = 1)
        {
            const int pageSize = 15;

            searchTerm = (searchTerm ?? "").Trim();

            var query = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                bool hasId = int.TryParse(
     searchTerm.TrimStart('#'),
     out int orderId);

                if (hasId)
                {
                    query = query.Where(o =>
                        o.OrderID == orderId ||
                        o.Customer.CustomerName.Contains(searchTerm));
                }
                else
                {
                    query = query.Where(o =>
                        o.Customer.CustomerName.Contains(searchTerm));
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                query = query.Where(
                    o => o.PaymentStatus == paymentStatus);
            }

            if (fromDate.HasValue)
            {
                DateTime start = fromDate.Value.Date;
                query = query.Where(o => o.OrderDate >= start);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date;

                query = query.Where(o =>
                    DbFunctions.TruncateTime(o.OrderDate) <= end);
            }

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                ModelState.AddModelError(
                    "",
                    "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
            }

            int totalItems = query.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize));

            page = Math.Max(1, Math.Min(page, totalPages));

            IOrderedQueryable<Order> ordered;

            switch (sort)
            {
                case "oldest":
                    ordered = query
                        .OrderBy(o => o.OrderDate)
                        .ThenBy(o => o.OrderID);
                    break;

                case "total_asc":
                    ordered = query
                        .OrderBy(o => o.TotalAmount)
                        .ThenByDescending(o => o.OrderID);
                    break;

                case "total_desc":
                    ordered = query
                        .OrderByDescending(o => o.TotalAmount)
                        .ThenByDescending(o => o.OrderID);
                    break;

                default:
                    sort = "newest";

                    ordered = query
                        .OrderByDescending(o => o.OrderDate)
                        .ThenByDescending(o => o.OrderID);
                    break;
            }

            return View(new AdminOrdersViewModel
            {
                Items = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),

                SearchTerm = searchTerm,
                Status = status,
                PaymentStatus = paymentStatus,
                FromDate = fromDate,
                ToDate = toDate,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(400);
            }

            Order order = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }

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

                    string[] allowed =
                        OrderStates.Next(order.OrderStatus);

                    bool isCancellation = status == "Đã hủy";

                    if (order.OrderStatus == "Đã hủy")
                    {
                        transaction.Rollback();

                        TempData["Error"] = "Đơn hàng này đã hủy trước đó.";

                        return RedirectToAction("Details", new { id });
                    }

                    if (!isCancellation && !allowed.Contains(status))

                        // Đơn đã thu tiền cần xử lý hoàn tiền riêng.
                        if (status == "Đã hủy" &&
                        order.PaymentStatus == "Đã thanh toán")
                    {
                       
                    }

                    if (isCancellation && order.OrderStatus != "Đã giao")
                    {
                        foreach (var item in order.OrderDetails)
                        {
                            ProductDetail stock = db.ProductDetails
                                .FirstOrDefault(d =>
                                    d.ProductID == item.ProductID);

                            if (stock == null ||
                                stock.QuantitySold < item.Quantity)
                            {
                                throw new InvalidOperationException(
                                    "Dữ liệu tồn kho không hợp lệ.");
                            }

                            stock.QuantitySold -= item.Quantity;
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

            return RedirectToAction(
                "Details",
                new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.OrderStatus == "Đã hủy")
            {
                TempData["Error"] =
                    "Không thể xác nhận thanh toán cho đơn đã hủy.";

                return RedirectToAction(
                    "Details",
                    new { id });
            }

            if (order.PaymentStatus == "Đã thanh toán")
            {
                TempData["Success"] =
                    "Đơn hàng đã được xác nhận thanh toán.";

                return RedirectToAction(
                    "Details",
                    new { id });
            }

            try
            {
                order.PaymentStatus = "Đã thanh toán";
                db.SaveChanges();

                TempData["Success"] =
                    "Đã xác nhận nhận đủ tiền.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                TempData["Error"] =
                    "Không thể cập nhật thanh toán.";
            }

            return RedirectToAction(
                "Details",
                new { id });
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