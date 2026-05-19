using BestoDokan.Application.DTOs;
using BestoDokan.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BestoDokan.Controllers
{
    [Authorize] // লগইন ছাড়া কেউ এই কন্ট্রোলারে ঢুকতে পারবে না
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ১. নিজের প্রোফাইল দেখা (Get Profile)
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            // কুকির টোকেন থেকে লগইন করা ইউজারের ID বের করার ডটনেট মেকানিজম
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User not found!" });

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Address = user.Address,
                City = user.City
            };

            return Ok(profileDto);
        }

        // ২. প্রোফাইল আপডেট করা (Update Profile)
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User not found!" });

            // নতুন ডাটা দিয়ে ইউজার অবজেক্ট আপডেট করা হচ্ছে
            user.FullName = updateDto.FullName;
            user.Address = updateDto.Address;
            user.City = updateDto.City;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Profile updated successfully!" });
        }
    }
}