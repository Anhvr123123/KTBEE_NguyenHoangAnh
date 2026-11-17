using System.ComponentModel.DataAnnotations;
using System.Web;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class CustomerProfileViewModel
    {
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(
            100,
            ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(
            15,
            ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [StringLength(
            100,
            ErrorMessage = "Email tối đa 100 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [StringLength(
            255,
            ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public HttpPostedFileBase Avatar { get; set; }
    }
}