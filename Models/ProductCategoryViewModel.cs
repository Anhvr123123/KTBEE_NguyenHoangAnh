using System;
using System.Collections.Generic;

namespace KTBEE_NguyenHoangAnh.Models
{
    public class ProductCategoryViewModel
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string SearchTerm { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public int? Year { get; set; }

        public string Sort { get; set; }

        public List<int> Years { get; set; }

        public List<Product> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages
        {
            get
            {
                return Math.Max(
                    1,
                    (int)Math.Ceiling(
                        TotalItems / (double)PageSize));
            }
        }
    }
}