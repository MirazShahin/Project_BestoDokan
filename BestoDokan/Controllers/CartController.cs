using BestoDokan.Application.DTOs;
using BestoDokan.Domain.Entities;
using BestoDokan.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BestoDokan.Controllers
{
    [Authorize] // 👈 কার্ট ব্যবহারের জন্য লগইন থাকা বাধ্যতামূলক
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ১. কাস্টমারের কার্ট দেখা (Get User Cart)
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // ইউজারের কার্ট এবং তার ভেতরের প্রোডাক্টের ইনফরমেশন ডাটাবেজ থেকে তুলে আনা
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Ok(new CartDto()); // কার্ট না থাকলে খালি কার্ট রিটার্ন করবে
            }

            var cartDto = new CartDto
            {
                Items = cart.CartItems.Select(ci => new CartItemItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return Ok(cartDto);
        }

        // ২. কার্টে প্রোডাক্ট যোগ করা (Add To Cart)
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // প্রোডাক্টটি আদৌ ডাটাবেজে আছে কিনা চেক করা
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound(new { message = "Product not found!" });

            // ইউজারের কোনো কার্ট অলরেডি তৈরি করা আছে কিনা দেখা
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // কার্ট না থাকলে নতুন কার্ট তৈরি করা
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // আইডি জেনারেট করার জন্য সেভ
            }

            // কার্টের ভেতর এই প্রোডাক্টটি অলরেডি কাস্টমার আগে যোগ করেছিল কিনা চেক করা
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                // প্রোডাক্ট অলরেডি থাকলে শুধু কোয়ান্টিটি বাড়িয়ে দেওয়া
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                // একদম নতুন প্রোডাক্ট হলে নতুন CartItem তৈরি করা
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Item added to cart successfully!" });
        }

        // ৩. কার্টের আইটেম কোয়ান্টিটি আপডেট করা (Update Quantity)
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0) return BadRequest(new { message = "Quantity must be greater than 0" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound(new { message = "Cart not found!" });

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null) return NotFound(new { message = "Product not found in your cart!" });

            cartItem.Quantity = quantity; // নতুন কোয়ান্টিটি সেট করা
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart updated successfully!" });
        }

        // ৪. কার্ট থেকে প্রোডাক্ট ডিলিট করা (Remove Item)
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound(new { message = "Cart not found!" });

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null) return NotFound(new { message = "Item not found in cart!" });

            _context.CartItems.Remove(cartItem); // কার্ট থেকে রিমুভ
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from cart successfully!" });
        }
    }
}