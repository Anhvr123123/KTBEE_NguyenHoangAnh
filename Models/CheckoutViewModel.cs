using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(
            100,
            ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(
            15,
            ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận xe.")]
        [StringLength(
            255,
            ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ nhận xe")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [RegularExpression(
            "^(COD|BANK)$",
            ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        public List<CartItem> Items { get; set; }

        public decimal TotalAmount { get; set; }
    }
}