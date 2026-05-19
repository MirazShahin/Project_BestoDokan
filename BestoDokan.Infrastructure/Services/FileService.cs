using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BestoDokan.Infrastructure.Services
{
    public class FileService
    {
        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // 👈 IWebHostEnvironment ছাড়া কারেন্ট প্রজেক্টের রুট থেকে wwwroot/images এর পাথ বের করার নিয়ম:
            var baseDirectory = AppContext.BaseDirectory;

            // সাধারণত ডিরেক্টরি bin/Debug ফোল্ডারে থাকে, তাই মেইন প্রজেক্ট ফোল্ডারে যাওয়ার জন্য পাথ ক্লিন করা
            var rootPath = baseDirectory.Substring(0, baseDirectory.IndexOf("bin"));
            var wwwrootPath = Path.Combine(rootPath, "wwwroot");
            var finalPath = Path.Combine(wwwrootPath, folderName);

            // ফোল্ডার না থাকলে অটোমেটিক তৈরি করবে
            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }

            // ফাইলের একটি ইউনিক নাম তৈরি করা (যেমন: guid-image.jpg)
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var fileNameWithPath = Path.Combine(finalPath, uniqueFileName);

            // ফাইলটি ফোল্ডারে সেভ করা
            using var stream = new FileStream(fileNameWithPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return uniqueFileName; // শুধু ইউনিক ফাইলের নামটা রিটার্ন করবে
        }
    }
}