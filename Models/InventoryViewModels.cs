using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class InventoryRowViewModel
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public string CategoryName { get; set; }

        public int Quantity { get; set; }

        public int QuantitySold { get; set; }

        public int Available
        {
            get
            {
                return Quantity - QuantitySold;
            }
        }
    }

    public class InventoryViewModel
    {
        public string SearchTerm { get; set; }

        public List<InventoryRowViewModel> InStock { get; set; }

        public List<InventoryRowViewModel> NeedImport { get; set; }
    }

    public class InventoryEditViewModel
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public int QuantitySold { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng số lượng.")]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0.")]
        public int? Quantity { get; set; }
    }
}