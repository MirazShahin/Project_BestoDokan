using BestoDokan.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // 👈 ১. Identity-র জন্য এই নতুন নেমস্পেসটি যোগ করা হলো
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BestoDokan.Infrastructure.Data
{
    // 🚀 ২. এখানে ম্যাজিক! DbContext এর বদলে IdentityDbContext<ApplicationUser> ব্যবহার করা হচ্ছে
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> /*এখানে আপনার ক্লাসটি EF Core-এর মূল ক্লাস 
                                                    DbContext-কে ইনহেরিট (inherit) করছে। 
                                                    এর ফলে আপনার এই ক্লাসটি ডাটাবেজ হ্যান্ডেল 
                                                    करनेর সব জাদুকরী ক্ষমতা (যেমন: ডাটা সেভ করা, 
                                                    কুয়েরি চালানো) পেয়ে যাচ্ছে।*/
    {
        /*এটি হলো এই ক্লাসের কনস্ট্রাক্টর। এটার কাজ 
          হলো Program.cs থেকে ডাটাবেজের কানেকশন 
          স্ট্রিং (Connection String) এবং আপনি 
          কোন ডাটাবেজ ব্যবহার করছেন (যেমন: SQL Server) \
          সেই সেটিংসগুলো গ্রহণ করে মেইন DbContext-এর 
          কাছে পাঠিয়ে দেওয়া।*/
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) // constructor
        {
        }
        public DbSet<Product> Products { get; set; } // DbSet মানে হলো ডাটাবেজের একটি টেবিল।
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        /*এই মেথডটি ব্যবহার করা হয় ডাটাবেজের টেবিলগুলোর 
          মধ্যে রিলেশনশিপ (যেমন: One-to-Many, Many-to-Many) 
          বা কোনো নির্দিষ্ট নিয়ম (Constraints) কোডের মাধ্যমে বলে 
          দেওয়ার জন্য। যেমন— কোনো কলামের সাইজ কত হবে 
          বা প্রাইমারি কি (PK) কোনটা হবে, তা আপনি এখানে কনফিগার করতে পারবেন।*/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🚀 ৩. Identity-র নিজস্ব টেবিল ম্যাপিং ব্যাকগ্রাউন্ডে ঠিক রাখার জন্য এটি অত্যন্ত জরুরি
            base.OnModelCreating(modelBuilder);
        }
    }
}


/*আপনি যদি DbContext ব্যবহার না করতেন, 
 তবে ডাটাবেজ থেকে একটা প্রোডাক্টের লিস্ট আনার 
জন্য আপনাকে অনেক বড় বড় SQL Query লিখতে হতো, 
কানেকশন ওপেন-ক্লোজ করতে হতো। 
ApplicationDbContext সেই কষ্ট দূর করে দেয়।*/