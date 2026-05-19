using BestoDokan.Application.Interfaces;
using BestoDokan.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BestoDokan.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        // appsettings.json থেকে সিক্রেট কি (Secret Key) পড়ার জন্য IConfiguration ইনজেক্ট করা হলো
        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            // ১. ক্লেইম বা ইউজারের পরিচিতি সেট করা (টোকেনের ভেতর কী কী তথ্য ইনকোড থাকবে)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // ২. appsettings থেকে সিক্রেট কি এনে সেটিকে ক্রিপ্টোগ্রাফির জন্য রেডি করা
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKeyDefault1234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ৩. টোকেনের এক্সপায়ারি ডেট এবং সিক্রেট কি দিয়ে মেইন টোকেন অবজেক্ট তৈরি করা
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // টোকেনটি ৭ দিন পর্যন্ত ভ্যালিড থাকবে
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            // ৪. টোকেনটিকে ফাইনাল স্ট্রিং-এ রূপান্তর করে রিটার্ন করা
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}