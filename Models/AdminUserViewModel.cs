using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class AdminUserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(
            50,
            MinimumLength = 4,
            ErrorMessage = "Tên đăng nhập từ 4 đến 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}