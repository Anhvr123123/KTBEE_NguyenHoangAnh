
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using KTBEE_NguyenHoangAnh.Models;

namespace KTBEE_NguyenHoangAnh.Controllers
{
    public class CartController : Controller
    {
        private readonly LuxuryAutoEntities db = new LuxuryAutoEntities();
        private List<CartItem> GetCart()
        {
            if (!(Session["Cart"] is List<CartItem> cart))
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }

            return cart;
        }

        public ActionResult Index()
        {
            return View(GetCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int id, int quantity = 1)
        {
            Product product = db.Products.Find(id);

            if (product == null)
                return HttpNotFound();

            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Details", "Home",
                    new { id });
            }

            var stock = db.ProductDetails
                .FirstOrDefault(d => d.ProductID == id);

            int available = stock == null
                ? 0
                : Math.Max(0, stock.Quantity - stock.QuantitySold);

            var cart = GetCart();

            CartItem existing = cart
                .FirstOrDefault(c => c.ProductID == id);

            int currentQuantity = existing == null
                ? 0
                : existing.Quantity;

            if (currentQuantity + quantity > available)
            {
                TempData["Error"] =
                    "Số lượng yêu cầu vượt quá số xe còn trong kho.";

                return RedirectToAction("Details", "Home",
                    new { id });
            }

            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ProductImage,
                    ProductPrice = product.ProductPrice,
                    Quantity = quantity
                });
            }
            else
            {
                existing.Quantity += quantity;
                existing.ProductName = product.ProductName;
                existing.ProductImage = product.ProductImage;
                existing.ProductPrice = product.ProductPrice;
            }

            Session["Cart"] = cart;
            TempData["Success"] = "Đã thêm xe vào giỏ hàng.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, int quantity)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(
                c => c.ProductID == id);

            if (item == null)
                return RedirectToAction("Index");

            if (quantity < 1)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(id);

            var stock = db.ProductDetails
                .FirstOrDefault(d => d.ProductID == id);

            if (product == null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;

                TempData["Error"] = "Xe không còn tồn tại.";
                return RedirectToAction("Index");
            }

            int available = stock == null
                ? 0
                : Math.Max(0, stock.Quantity - stock.QuantitySold);

            if (quantity > available)
            {
                TempData["Error"] =
                    "Số lượng vượt quá tồn kho. Hiện còn " +
                    available + " xe.";

                return RedirectToAction("Index");
            }

            item.Quantity = quantity;
            item.ProductName = product.ProductName;
            item.ProductPrice = product.ProductPrice;
            item.ProductImage = product.ProductImage;

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var cart = GetCart();

            var item = cart
                .FirstOrDefault(c => c.ProductID == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            TempData["Success"] =
                "Đã xóa sản phẩm khỏi giỏ hàng.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            Session["Cart"] = new List<CartItem>();

            TempData["Success"] =
                "Đã xóa toàn bộ giỏ hàng.";

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
