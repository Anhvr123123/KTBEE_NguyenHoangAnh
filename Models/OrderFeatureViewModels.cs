using System;
using System.Collections.Generic;

namespace KTBEE_NguyenHoangAnh.Models
{
    public static class OrderStates
    {
        public static readonly string[] All =
        {
        "Chờ xác nhận",
        "Đang xử lý",
        "Đang vận chuyển",
        "Đã giao",
        "Đã hủy"
    };

        public static string[] Next(string current)
        {
            switch ((current ?? "").Trim())
            {
                // Hỗ trợ cả các đơn cũ.
                case "Chờ thanh toán":
                case "Chờ xác nhận":
                    return new[]
                    {
                    "Đang xử lý",
                    "Đã hủy"
                };

                case "Đang xử lý":
                    return new[]
                    {
                    "Đang vận chuyển",
                    "Đã hủy"
                };

                case "Đang vận chuyển":
                    return new[]
                    {
                    "Đã giao"
                };

                default:
                    return new string[0];
            }
        }
    }

    public class AdminOrdersViewModel
    {
        public List<Order> Items { get; set; }

        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Sort { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

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

    public class CustomerOrderGroup
    {
        public string Status { get; set; }
        public int Total { get; set; }
        public List<Order> Items { get; set; }
    }

    public class MyOrdersViewModel
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public bool Expanded { get; set; }

        public int TotalOrders { get; set; }

        public List<CustomerOrderGroup> Groups { get; set; }
    }
}