
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [Display(Name = "Địa chỉ nhận xe")]
        public string ShippingAddress { get; set; }

        public string PaymentMethod { get; set; }

        public List<CartItem> Items { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
