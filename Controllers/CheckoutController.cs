
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly LuxuryAutoEntities db = new LuxuryAutoEntities();
        private List<CartItem> GetCart()
        {
            return Session["Cart"] as List<CartItem>
                   ?? new List<CartItem>();
        }

        private Customer GetCurrentCustomer()
        {
            string username =
                Convert.ToString(Session["Username"]);

            string role =
                Convert.ToString(Session["UserRole"]);

            if (string.IsNullOrWhiteSpace(username)
                || role != "C")
            {
                return null;
            }

            return db.Customers.FirstOrDefault(
                c => c.Username == username);
        }

        [HttpGet]
        public ActionResult Index()
        {
            Customer customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login", "Account");
            }

            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["Error"] =
                    "Giỏ hàng đang trống.";

                return RedirectToAction(
                    "Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                ShippingAddress = customer.CustomerAddress,
                PaymentMethod = "COD",
                Items = cart,
                TotalAmount = cart.Sum(x => x.TotalPrice)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel model)
        {
            Customer customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login", "Account");
            }

            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["Error"] =
                    "Giỏ hàng đang trống.";

                return RedirectToAction(
                    "Index", "Cart");
            }

            model.Items = cart;
            model.TotalAmount =
                cart.Sum(x => x.TotalPrice);

            if (!ModelState.IsValid)
                return View(model);

            if (model.PaymentMethod != "COD" && model.PaymentMethod != "BANK")
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Phương thức thanh toán không hợp lệ.");

                return View(model);
            }

            using (var transaction =
                db.Database.BeginTransaction(
                    IsolationLevel.Serializable))
            {
                try
                {
                    decimal total = 0;

                    var orderItems =
                        new List<OrderDetail>();

                    foreach (var item in cart)
                    {
                        if (item.Quantity <= 0)
                        {
                            throw new InvalidOperationException(
                                "Số lượng xe không hợp lệ.");
                        }

                        Product product = db.Products
                            .FirstOrDefault(p => p.ProductID == item.ProductID)
                            ?? throw new InvalidOperationException(
                                "Có xe không còn tồn tại.");

                        ProductDetail stock = db.ProductDetails
                            .FirstOrDefault(
                                d => d.ProductID == item.ProductID);

                        int available = stock == null
                            ? 0
                            : stock.Quantity - stock.QuantitySold;

                        if (item.Quantity > available)
                        {
                            throw new InvalidOperationException(
                                "Xe " + product.ProductName +
                                " không đủ số lượng trong kho.");
                        }

                        decimal unitPrice =
                            product.ProductPrice;

                        decimal lineTotal =
                            unitPrice * item.Quantity;

                        total += lineTotal;

                        orderItems.Add(new OrderDetail
                        {
                            ProductID = product.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = lineTotal
                        });

                        stock.QuantitySold += item.Quantity;
                    }



                    Order order = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Now,
                        TotalAmount = total,
                        PaymentMethod = model.PaymentMethod,
                        PaymentStatus = "Chưa thanh toán",
                        OrderStatus = "Chờ thanh toán",
                        ShippingMethod = "Giao xe",
                        ShippingAddress = model.ShippingAddress.Trim()
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();



                    foreach (var detail in orderItems)
                    {
                        detail.OrderID = order.OrderID;
                        db.OrderDetails.Add(detail);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    int orderId = order.OrderID;

                    Session["Cart"] =
                        new List<CartItem>();

                    return RedirectToAction(
                        "Success", new { id = orderId });
                }
                catch (InvalidOperationException ex)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "", ex.Message);
                }

                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Debug.WriteLine(
                        ex.ToString());

                    string error = ex.Message;

                    if (ex.InnerException != null)
                    {
                        error += " | " + ex.InnerException.Message;

                        if (ex.InnerException.InnerException != null)
                        {
                            error += " | " +
                                ex.InnerException.InnerException.Message;
                        }
                    }

                    ModelState.AddModelError("", error);
                }

            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Success(int id)
        {
            Customer customer = GetCurrentCustomer();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login", "Account");
            }

            Order order = db.Orders
                .AsNoTracking()
                .FirstOrDefault(o =>
                    o.OrderID == id &&
                    o.CustomerID == customer.CustomerID);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
