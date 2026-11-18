using System;
using System.Collections.Generic;
using System.Data;
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

        private Customer CurrentCustomer()
        {
            string username =
                Convert.ToString(Session["Username"]);

            string role =
                Convert.ToString(Session["UserRole"]).Trim();

            if (string.IsNullOrWhiteSpace(username) ||
                role != "C")
            {
                return null;
            }

            return db.Customers.FirstOrDefault(
                c => c.Username == username &&
                     c.User.UserRole.Trim() == "C");
        }

        private ActionResult LoginRequired()
        {
            if (string.IsNullOrWhiteSpace(
                Convert.ToString(Session["Username"])))
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            return new HttpStatusCodeResult(
                403,
                "Vui lòng sử dụng tài khoản khách hàng.");
        }

        // DANH SÁCH ĐƠN HÀNG
        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string status,
            bool expanded = false)
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return LoginRequired();
            }

            searchTerm = (searchTerm ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(status) &&
                !OrderStates.All.Contains(status))
            {
                return new HttpStatusCodeResult(
                    400,
                    "Trạng thái không hợp lệ.");
            }

            var query = db.Orders
                .AsNoTracking()
                .Where(o =>
                    o.CustomerID == customer.CustomerID);

            int totalOrders = query.Count();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string idText = searchTerm.TrimStart('#');

                bool hasId = int.TryParse(idText, out int orderId);
                if (hasId)
                {
                    query = query.Where(o =>
                        o.OrderID == orderId ||
                        o.OrderDetails.Any(d =>
                            d.Product.ProductName.Contains(
                                searchTerm)));
                }
                else
                {
                    query = query.Where(o =>
                        o.OrderDetails.Any(d =>
                            d.Product.ProductName.Contains(
                                searchTerm)));
                }
            }

            string[] selectedStates =
                string.IsNullOrWhiteSpace(status)
                    ? OrderStates.All
                    : new[] { status };

            var groups =
                new List<CustomerOrderGroup>();

            foreach (string state in selectedStates)
            {
                var groupQuery = query.Where(
                    o => o.OrderStatus == state);

                int count = groupQuery.Count();

                int limit =
                    !string.IsNullOrWhiteSpace(status) &&
                    expanded
                        ? 50
                        : 2;

                var items = groupQuery
                    .Include(o =>
                        o.OrderDetails.Select(d => d.Product))
                    .OrderByDescending(o => o.OrderDate)
                    .ThenByDescending(o => o.OrderID)
                    .Take(limit)
                    .ToList();

                groups.Add(new CustomerOrderGroup
                {
                    Status = state,
                    Total = count,
                    Items = items
                });
            }

            return View(new MyOrdersViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                Expanded = expanded,
                TotalOrders = totalOrders,
                Groups = groups
            });
        }

        // CHI TIẾT / HÓA ĐƠN
        [HttpGet]
        public ActionResult Details(int id)
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return LoginRequired();
            }

            Order order = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o =>
                    o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.CustomerID == customer.CustomerID);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // MUA LẠI: THÊM 1 XE VÀO GIỎ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(
            int orderId,
            int productId)
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return LoginRequired();
            }

            bool ownedProduct = db.OrderDetails.Any(d =>
                d.OrderID == orderId &&
                d.ProductID == productId &&
                d.Order.CustomerID == customer.CustomerID);

            if (!ownedProduct)
            {
                return HttpNotFound();
            }

            Product product =
                db.Products.Find(productId);

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm này không còn tồn tại.";

                return RedirectToAction("Index");
            }

            ProductDetail stock = db.ProductDetails
                .FirstOrDefault(d =>
                    d.ProductID == productId);

            int available = stock == null
                ? 0
                : Math.Max(
                    0,
                    stock.Quantity - stock.QuantitySold);

            var cart = Session["Cart"] as List<CartItem>
                       ?? new List<CartItem>();

            CartItem existing = cart.FirstOrDefault(
                c => c.ProductID == productId);

            int currentQuantity = existing == null
                ? 0
                : existing.Quantity;

            if (currentQuantity >= available)
            {
                TempData["Error"] =
                    "Xe không đủ số lượng để mua thêm.";

                return RedirectToAction("Index");
            }

            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    ProductPrice = product.ProductPrice,
                    Quantity = 1
                });
            }
            else
            {
                existing.Quantity += 1;
                existing.ProductName = product.ProductName;
                existing.ProductImage = product.ProductImage;
                existing.ProductPrice = product.ProductPrice;
            }

            Session["Cart"] = cart;

            TempData["Success"] =
                "Đã thêm 1 xe vào giỏ hàng.";

            return RedirectToAction(
                "Index",
                "Cart",
                new { area = "" });
        }

        // HỦY ĐƠN HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            Customer customer = CurrentCustomer();

            if (customer == null)
            {
                return LoginRequired();
            }

            using (var transaction =
                db.Database.BeginTransaction(
                    IsolationLevel.Serializable))
            {
                try
                {
                    // Chỉ được hủy đơn thuộc tài khoản hiện tại.
                    Order order = db.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault(o =>
                            o.OrderID == id &&
                            o.CustomerID == customer.CustomerID);

                    if (order == null)
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Không tìm thấy đơn hàng thuộc tài khoản của bạn.";

                        return RedirectToAction("Index");
                    }

                    string previousStatus =
                        (order.OrderStatus ?? "").Trim();

                    // Chặn hủy lặp, tránh hoàn kho nhiều lần.
                    if (previousStatus == "Đã hủy")
                    {
                        transaction.Rollback();

                        TempData["Error"] =
                            "Đơn hàng này đã được hủy trước đó.";

                        return RedirectToAction("Index");
                    }

                    bool releaseStock =
                        previousStatus == "Chờ thanh toán" ||
                        previousStatus == "Chờ xác nhận" ||
                        previousStatus == "Đang xử lý";

                    // Xe chưa giao đi: giải phóng số lượng giữ.
                    // Xe đang vận chuyển / đã giao:
                    // chỉ nhập lại kho khi thực tế nhận xe trả.
                    if (releaseStock)
                    {
                        foreach (var item in order.OrderDetails)
                        {
                            ProductDetail stock = db.ProductDetails
                                .FirstOrDefault(d =>
                                    d.ProductID == item.ProductID);

                            if (item.Quantity <= 0 ||
                                stock == null ||
                                stock.QuantitySold < item.Quantity)
                            {
                                throw new InvalidOperationException(
                                    "Dữ liệu tồn kho không hợp lệ.");
                            }

                            stock.QuantitySold -= item.Quantity;
                        }
                    }

                    order.OrderStatus = "Đã hủy";

                    // Giữ nguyên PaymentStatus.
                    // Hủy đơn không đồng nghĩa đã hoàn tiền.
                    db.SaveChanges();
                    transaction.Commit();

                    string message =
                        "Đã hủy đơn hàng #" + order.OrderID + ".";

                    if (previousStatus == "Đang vận chuyển" ||
                        previousStatus == "Đã giao")
                    {
                        message +=
                            " Vui lòng liên hệ cửa hàng để xử lý trả xe.";
                    }

                    if (order.PaymentStatus == "Đã thanh toán")
                    {
                        message +=
                            " Vui lòng liên hệ cửa hàng để xử lý hoàn tiền.";
                    }

                    TempData["Success"] = message;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(ex);

                    TempData["Error"] =
                        "Không thể hủy đơn hàng. Vui lòng liên hệ cửa hàng hoặc thử lại.";
                }
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