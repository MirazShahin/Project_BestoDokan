using System.Collections.Generic;
using System.Linq;

namespace BestoDokan.Application.DTOs
{
    public class CartDto
    {
        public List<CartItemItemDto> Items { get; set; } = new List<CartItemItemDto>();
        public decimal GrandTotal => Items.Sum(x => x.TotalPrice); // কার্টের সব প্রোডাক্টের মোট খরচ
    }
}