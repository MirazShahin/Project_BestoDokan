using BestoDokan.Application.DTOs;
using BestoDokan.Domain.Entities;
using BestoDokan.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestoDokan.Controllers
{
    [Authorize(Roles = "Admin")] //শুধুমাত্র এডমিনরাই এই কন্ট্রোলারে এক্সেস পাবে
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // ১. টোটাল প্রোডাক্ট এবং ক্যাটাগরি কাউন্ট
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();

            // নতুন বা পেন্ডিং অর্ডার কয়টা আছে তার কাউন্ট
            var pendingOrders = await _context.Orders.Where(o => o.Status == "Pending").CountAsync();
            // ২. রোল অনুযায়ী ইউজার কাউন্ট করা
            var totalCustomers = 0;
            var totalAdmins = 0;

            // মোট কত টাকার সেল হলো (যেসব অর্ডার কাস্টমার বা অ্যাডমিন Cancel করেনি সেগুলোর যোগফল)
            var totalSales = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);
            // ডাটাবেজের সব ইউজারদের লুপ চালিয়ে চেক করা কে কোন রোলে আছে
            var allUsers = await _userManager.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    totalAdmins++;
                }
                else if (await _userManager.IsInRoleAsync(user, "Customer"))
                {
                    totalCustomers++;
                }
            }

            // ৩. সব ডেটা ডিটিও-তে বাইন্ড করে পাঠানো
            var statsDto = new AdminDashboardStatsDto
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalCustomers = totalCustomers,
                TotalAdmins = totalAdmins,
                TotalOrders = totalOrders,      
                PendingOrders = pendingOrders, 
                TotalSales = totalSales
            };

            return Ok(statsDto);
        }
    }
}