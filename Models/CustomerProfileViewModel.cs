using System.ComponentModel.DataAnnotations;
using System.Web;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class CustomerProfileViewModel
    {
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string CustomerPhone { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string CustomerEmail { get; set; }

        [StringLength(255)]
        public string CustomerAddress { get; set; }

        public HttpPostedFileBase Avatar { get; set; }
    }
}