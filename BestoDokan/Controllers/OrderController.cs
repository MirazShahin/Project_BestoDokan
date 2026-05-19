using BestoDokan.Application.DTOs;
using BestoDokan.Domain.Entities;
using BestoDokan.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BestoDokan.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ১. চেকআউট এবং অর্ডার প্লেস করা (Customer Only)
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // কাস্টমারের কার্ট খুঁজে বের করা এবং কার্ট আইটেমের সাথে প্রোডাক্টের দাম ইনক্লুড করা
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(new { message = "Your cart is empty! Cannot place an order." });
            }

            // নতুন অর্ডার অবজেক্ট তৈরি
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = dto.ShippingAddress,
                ShippingCity = dto.ShippingCity,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // অর্ডারের মেইন আইডি জেনারেট করার জন্য সেভ

            // কার্টের আইটেমগুলোকে অর্ডারের ডিটেইলস টেবিলে ট্রান্সফার করা
            foreach (var cartItem in cart.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price // কারেন্ট প্রাইস স্ন্যাপশট লক করা হলো
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // অর্ডার হয়ে যাওয়ার পর কাস্টমারের কার্ট খালি (Delete) করে দেওয়া
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order placed successfully!", orderId = order.Id });
        }

        // ২. কাস্টমার নিজের অর্ডারের লিস্ট দেখবে (Customer Only)
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // ৩. অ্যাডমিন পুরো সাইটের সব কাস্টমারের সব অর্ডার দেখবে (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // ৪. অ্যাডমিন অর্ডারের স্ট্যাটাস আপডেট করবে (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound(new { message = "Order not found!" });

            // স্ট্যাটাস আপডেট (যেমন: Shipped, Delivered)
            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Order status updated to {newStatus} successfully!" });
        }
    }
}