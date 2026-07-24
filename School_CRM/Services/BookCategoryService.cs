using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class BookCategoryService : IBookCategoryService
    {
        private readonly IBookCategoryRepository _repo;

        public BookCategoryService(IBookCategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<LibBookCategory>> GetAllCategoriesAsync(bool activeOnly = true)
        {
            return await _repo.GetAllAsync(activeOnly);
        }

        public async Task<LibBookCategory?> GetCategoryByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> CreateCategoryAsync(BookCategoryDto dto, int createdBy)
        {
            if (await _repo.ExistsAsync(dto.CategoryName))
                return (false, $"Category '{dto.CategoryName}' already exists.");

            var category = new LibBookCategory
            {
                CategoryName = dto.CategoryName.Trim(),
                Description  = dto.Description?.Trim(),
                IsActive     = dto.IsActive,
                CreatedAt    = DateTime.Now,
                CreatedBy    = createdBy
            };

            await _repo.CreateAsync(category);
            return (true, "Category created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(BookCategoryDto dto, int updatedBy)
        {
            var category = await _repo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return (false, "Category not found.");

            if (await _repo.ExistsAsync(dto.CategoryName, dto.CategoryId))
                return (false, $"Category '{dto.CategoryName}' already exists.");

            category.CategoryName = dto.CategoryName.Trim();
            category.Description  = dto.Description?.Trim();
            category.IsActive     = dto.IsActive;

            await _repo.UpdateAsync(category);
            return (true, "Category updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return result
                ? (true, "Category deactivated successfully.")
                : (false, "Cannot delete category. It may have books assigned or does not exist.");
        }
    }
}
