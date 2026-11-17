using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(
            50,
            ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
    }

    public class RegisterViewModel
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

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare(
            "Password",
            ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}