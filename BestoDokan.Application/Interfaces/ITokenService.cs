using BestoDokan.Domain.Entities;
using System.Threading.Tasks;
namespace BestoDokan.Application.Interfaces
{
    public interface ITokenService
    {
        // এই মেথডটি একজন ইউজার অবজেক্ট নিবে
        // এবং তার বিপরীতে একটি সিকিউরড JWT স্ট্রিং তৈরি করে দেবে
        Task<string> GenerateJwtToken(ApplicationUser user);
    }
}