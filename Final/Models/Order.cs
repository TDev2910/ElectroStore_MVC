using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ShippingMethod { get; set; }  // Giao hàng tận nơi hoặc hẹn lấy tại cửa hàng
        public string City { get; set; }
        public string Store { get; set; }
        public string Notes { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        // ✅ Danh sách sản phẩm trong đơn hàng
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}