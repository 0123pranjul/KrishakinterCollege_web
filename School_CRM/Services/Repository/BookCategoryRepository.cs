using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class BookCategoryRepository : IBookCategoryRepository
    {
        private readonly LibmanagementContext _context;

        public BookCategoryRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<List<LibBookCategory>> GetAllAsync(bool activeOnly = true)
        {
            var query = _context.LibBookCategories.AsQueryable();
            
            if (activeOnly)
                query = query.Where(x => x.IsActive);
            
            return await query
                .OrderBy(x => x.CategoryName)
                .ToListAsync();
        }

        public async Task<LibBookCategory?> GetByIdAsync(int id)
        {
            return await _context.LibBookCategories
                .FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<bool> ExistsAsync(string name, int excludeId = 0)
        {
            return await _context.LibBookCategories
                .AnyAsync(x => x.CategoryName == name && x.CategoryId != excludeId);
        }

        public async Task<LibBookCategory> CreateAsync(LibBookCategory category)
        {
            _context.LibBookCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<LibBookCategory> UpdateAsync(LibBookCategory category)
        {
            _context.LibBookCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category == null) return false;

            // Check if any books exist in this category
            var hasBooks = await _context.LibBooks
                .AnyAsync(x => x.CategoryId == id && x.IsActive);
            
            if (hasBooks) return false;

            category.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
