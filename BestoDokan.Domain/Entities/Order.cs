using System;
using System.Collections.Generic;

namespace BestoDokan.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        // শিপিং ডিটেইলস
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;

        // অর্ডার স্ট্যাটাস (যেমন: Pending, Shipped, Delivered, Cancelled)
        public string Status { get; set; } = "Pending";

        // One-to-Many Relationship
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}