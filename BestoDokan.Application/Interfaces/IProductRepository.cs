using BestoDokan.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BestoDokan.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IReadOnlyList<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<int> SaveChangesAsync();

        /*Task হলো এমন একটি প্রতিশ্রুতি বা "Promise" 
          যে ভবিষ্যতে আপনাকে একটি রেজাল্ট দেওয়া হবে।
          Task (বিনা টাইপ): যদি মেথডটি কোনো ডাটা 
          রিটার্ন না করে, তখন শুধু Task ব্যবহার করা হয়। 
          এটি অনেকটা void-এর মতো, তবে অ্যাসিনক্রোনাস।*/

        /* Async মানে হলো কোনো কাজ করার সময় 
          অ্যাপ্লিকেশনকে "আটকে না রাখা" (Non-blocking)।
          স্বাভাবিক প্রোগ্রামে একটি কাজ শেষ না হওয়া পর্যন্ত 
          পরের কাজ শুরু হয় না (Synchronous)। 
          কিন্তু ডাটাবেজ থেকে ডাটা আনা বা নেটওয়ার্ক কলের 
          মতো কাজে সময় লাগে। Async ব্যবহার করলে ডাটাবেজ 
          যখন ডাটা প্রসেস করে, তখন আপনার অ্যাপের মেইন থ্রেড 
          বা প্রসেসর বসে না থেকে অন্য কাজ 
          (যেমন: অন্য ইউজারের রিকোয়েস্ট হ্যান্ডেল করা) করতে পারে।*/
    }
}