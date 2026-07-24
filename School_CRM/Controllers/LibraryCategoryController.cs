using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Library/Category")]
    public class LibraryCategoryController : Controller
    {
        private readonly IBookCategoryService _categoryService;

        public LibraryCategoryController(IBookCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync(false);
            return View(categories);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new BookCategoryDto { IsActive = true });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            var (success, message) = await _categoryService.CreateCategoryAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", message);
            return View(dto);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            var dto = new BookCategoryDto
            {
                CategoryId   = category.CategoryId,
                CategoryName = category.CategoryName,
                Description  = category.Description,
                IsActive     = category.IsActive
            };

            return View(dto);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCategoryDto dto)
        {
            if (id != dto.CategoryId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            var (success, message) = await _categoryService.UpdateCategoryAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", message);
            return View(dto);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _categoryService.DeleteCategoryAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }
    }
}
