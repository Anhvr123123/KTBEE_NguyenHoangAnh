using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên hãng xe.")]
        [StringLength(
            100,
            ErrorMessage = "Tên hãng tối đa 100 ký tự.")]
        [Display(Name = "Tên hãng xe")]
        public string CategoryName { get; set; }

        [StringLength(
            500,
            ErrorMessage = "Mô tả tối đa 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
    }

    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên xe.")]
        [StringLength(
            150,
            ErrorMessage = "Tên xe tối đa 150 ký tự.")]
        [Display(Name = "Tên xe")]
        public string ProductName { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn hãng xe.")]
        [Display(Name = "Hãng xe")]
        public int CategoryID { get; set; }

        [Range(
            typeof(decimal),
            "1",
            "9999999999999999",
            ErrorMessage = "Giá bán phải lớn hơn 0 và trong phạm vi cho phép.")]
        [Display(Name = "Giá bán")]
        public decimal ProductPrice { get; set; }

        [StringLength(
            500,
            ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự.")]
        [Display(Name = "Ảnh xe")]
        public string ProductImage { get; set; }

        [Range(
            1886,
            2100,
            ErrorMessage = "Năm sản xuất không hợp lệ.")]
        [Display(Name = "Năm sản xuất")]
        public int? ModelYear { get; set; }

        [StringLength(
            50,
            ErrorMessage = "Màu sắc tối đa 50 ký tự.")]
        [Display(Name = "Màu sắc")]
        public string CarColor { get; set; }

        [StringLength(
            100,
            ErrorMessage = "Thông tin động cơ tối đa 100 ký tự.")]
        [Display(Name = "Động cơ")]
        public string Engine { get; set; }

        [StringLength(
            100,
            ErrorMessage = "Thông tin hộp số tối đa 100 ký tự.")]
        [Display(Name = "Hộp số")]
        public string Transmission { get; set; }
    }
}