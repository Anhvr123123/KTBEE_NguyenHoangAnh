
using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class AdminRegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MinLength(4, ErrorMessage = "Tên đăng nhập tối thiểu 4 ký tự.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
