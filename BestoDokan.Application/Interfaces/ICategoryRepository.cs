using BestoDokan.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BestoDokan.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IReadOnlyList<Category>> GetAllAsync();
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<int> SaveChangesAsync();
    }
}