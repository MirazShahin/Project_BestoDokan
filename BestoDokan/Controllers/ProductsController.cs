using Mapster;
using Microsoft.AspNetCore.Authorization;
using BestoDokan.Application.Interfaces;
using BestoDokan.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using BestoDokan.Application.DTOs;
using FluentValidation; 

/*
    Mapster হলো .NET-এর একটা সুপার-ফাস্ট টুল, 
    যা এক ধরনের অবজেক্ট থেকে অন্য অবজেক্টে 
    (যেমন: Database Entity থেকে DTO-তে) ডেটা কপি বা 
    টান্সফার করার কাজটা অটোমেটিক করে দেয়।

মূল সুবিধাগুলো:
   1) কোড কমে যায়
   2) নাম মিললে অটো ম্যাপিং
   3) ভুল কম হয়
   4) প্রচণ্ড ফাস্ট
*/

namespace BestoDokan.Controllers
{
    //[ApiController] and ControllerBase------ input validation er kaj kore dey

    [ApiController] // এই ক্লাসটি যে একটি Web API কন্ট্রোলার,
                    // তা .NET-কে বোঝানোর জন্য এটি ব্যবহার করা হয়েছে।

    [Route("api/[controller]")] // API এর URL হবে: api/products
    public class ProductsController : ControllerBase // এই ক্লাসটি যে একটি
                                                     // Web API কন্ট্রোলার, তা .NET-কে বোঝানোর জন্য
                                                     // এটি ব্যবহার করা হয়েছে।
    {
        private readonly IProductRepository _repository;
        private readonly IValidator<ProductDto> _validator;

        // DI মাধ্যমে Repositories নিয়ে আসা হচ্ছে
        public ProductsController(IProductRepository repository, IValidator<ProductDto> validator) /// etaa ekta constructor
        {
            _repository = repository; // এখানে আপনি সরাসরি ডাটাবেজের
                                      // ApplicationDbContext বা Entity Framework ব্যবহার না করে
                                      // IProductRepository (Interface) ব্যবহার করেছেন।
                                      // etai loosely coupled
            _validator = validator;   //ভ্যালিডেটর ইনজেক্ট করা হলো
        }


        //IActionResult এর সুবিধা হলো,
        //এটি দিয়ে আমরা বিভিন্ন রকমের HTTP রেসপন্স
        //(যেমন: Ok, NotFound, BadRequest) রিটার্ন করতে পারি।
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _repository.GetAllAsync(); // ডাটাবেজ থেকে ডাটা আনা..|| এটি ডাটাবেজে গিয়ে
                                                            // একটি SELECT * FROM Products কুয়েরি
                                                            // চালায় এবং সব প্রোডাক্টের ডাটা নিয়ে আসে।

            var productDtos = products.Adapt<IEnumerable<ProductDto>>();

            return Ok(productDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            { 
                return NotFound(new { message = "Product is not found...!!" });
            }

            var productDto = product.Adapt<ProductDto>();

            return Ok(productDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            // ডাটাবেজে যাওয়ার আগে FluentValidation রান করে চেক করা
            var validationResult = await _validator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var productEntity = productDto.Adapt<Product>();

            await _repository.AddAsync(productEntity); //এর কাজ হলো ডাটাটিকে লাইনে দাঁড় করানো।
            await _repository.SaveChangesAsync(); // এই LINE টি হলো আসল পাওয়ারহাউজ, যা সরাসরি ডাটাবেজ (SQL Server)-এর সাথে যোগাযোগ করে।


            productDto.Id = productEntity.Id; /* ক্লায়েন্ট যখন ডাটা পাঠিয়েছিল,
                                               তখন productDto.Id ছিল 0 বা ফাঁকা।
                                               কিন্তু এখন ডাটাবেজে সেভ হওয়ার পর
                                               আমরা আসল আইডিটি পেয়ে গেছি
                                               (ধরুন আইডিটি হলো 105)। আমরা
                                               সেই 105 আইডিটি আমাদের productDto.Id-এর
                                               ভেতরে সেট করে দিলাম, যেন ক্লায়েন্ট
                                                জানতে পারে তার তৈরি করা প্রোডাক্টের আইডি কত হয়েছে। */

            return CreatedAtAction(nameof(GetProduct), new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest(new { message = "Not matched..!" });
            }

            // আপডেট রিকোয়েস্টের জন্যও ডাটা ভ্যালিড কি না তা আগে চেক করা
            var validationResult = await _validator.ValidateAsync(productDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Products not found...!" });
            }

            productDto.Adapt(existingProduct);

            await _repository.UpdateAsync(existingProduct);
            await _repository.SaveChangesAsync();

            return NoContent();

            /* ১. ইউজার রিকোয়েস্ট দিলো 
               ২. আইডি চেক হলো 
               ৩. ডাটাবেজ থেকে ওল্ড ডাটা আনা হলো
               ৪. ওল্ড ডাটার ওপর নিউ ডাটা ম্যাপ (Adapt) করা হলো 
               ৫. ডাটাবেজে ফাইনাল সেভ হলো 
               ৬. সাকসেসফুল মেসেজ (204) গেলো।
             */
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found to delete." });
            }

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully." });
        }
    }
}
/*
   এই মেথডটি ক্লায়েন্ট থেকে প্রোডাক্টের 
   তথ্য নেয় ➡️ সেটিকে ডাটাবেজের ভাষায় (Entity) রূপান্তর করে 
   ➡️ ডাটাবেজে সেভ করে নতুন আইডি জেনারেট করে ➡️ 
   এবং সবশেষে আইডি সমেত পুরো প্রোডাক্টের 
   ডাটাটি 201 Created মেসেজসহ ফ্রন্টএন্ডে ফেরত পাঠায়।
*/