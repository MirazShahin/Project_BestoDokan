using Microsoft.AspNetCore.Authorization;
using BestoDokan.Application.DTOs;
using BestoDokan.Application.Interfaces;
using BestoDokan.Domain.Entities;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BestoDokan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;
        private readonly IValidator<CategoryDto> _validator; 

        // কনস্ট্রাক্টরে ভ্যালিডেটর ইনজেক্ট করা হলো
        public CategoriesController(ICategoryRepository repository, IValidator<CategoryDto> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _repository.GetAllAsync();
            var categoryDtos = categories.Adapt<IEnumerable<CategoryDto>>();
            return Ok(categoryDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            var categoryDto = category.Adapt<CategoryDto>();
            return Ok(categoryDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            // নতুন ক্যাটাগরি তৈরির আগে FluentValidation রান করা হচ্ছে
            var validationResult = await _validator.ValidateAsync(categoryDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary()); // রুল ব্রেক করলে এরর রিটার্ন করবে
            }

            var categoryEntity = categoryDto.Adapt<Category>();

            await _repository.AddAsync(categoryEntity);
            await _repository.SaveChangesAsync();

            categoryDto.Id = categoryEntity.Id;

            return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            // আপডেট করার আগেও ভ্যালিডেশন চেক করে নেওয়া নিরাপদ
            var validationResult = await _validator.ValidateAsync(categoryDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var existingCategory = await _repository.GetByIdAsync(id);

            if (existingCategory == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            categoryDto.Adapt(existingCategory);

            await _repository.UpdateAsync(existingCategory);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existingCategory = await _repository.GetByIdAsync(id);

            if (existingCategory == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found to delete." });
            }

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}