using Microsoft.AspNetCore.Identity;

namespace BestoDokan.Domain.Entities
{
    // IdentityUser-কে ইনহেরিট (Inherit) করার কারণে ডটনেটের সব ডিফল্ট ইউজার ফিল্ড আমরা পেয়ে যাবো
    public class ApplicationUser : IdentityUser
    {
        // ই-কমার্সের জন্য আমাদের কাস্টম অতিরিক্ত ফিল্ডসমূহ
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}