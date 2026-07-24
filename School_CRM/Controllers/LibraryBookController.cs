using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Library/Book")]
    public class LibraryBookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBookCategoryService _categoryService;

        public LibraryBookController(IBookService bookService, IBookCategoryService categoryService)
        {
            _bookService     = bookService;
            _categoryService = categoryService;
        }

        // ============================================================
        // BOOK LIST / SEARCH
        // ============================================================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(BookSearchDto filter)
        {
            var (items, totalCount) = await _bookService.SearchBooksAsync(filter);
            
            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId", "CategoryName");
            
            ViewBag.TotalCount  = totalCount;
            ViewBag.CurrentPage = filter.PageNumber;
            ViewBag.PageSize    = filter.PageSize;
            ViewBag.TotalPages  = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            
            return View(items);
        }

        // ============================================================
        // CREATE BOOK
        // ============================================================
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId", "CategoryName");
            
            return View(new BookDto { Language = "Hindi", NumberOfCopies = 1 });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _categoryService.GetAllCategoriesAsync(),
                    "CategoryId", "CategoryName");
                return View(dto);
            }

            var userId = GetCurrentUserId();
            var (success, message, bookId) = await _bookService.CreateBookAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Copies), new { id = bookId });
            }

            TempData["Error"] = message;
            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId", "CategoryName");
            return View(dto);
        }

        // ============================================================
        // EDIT BOOK
        // ============================================================
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var dto = new BookDto
            {
                BookId        = book.BookId,
                ISBN          = book.Isbn,
                Title         = book.Title,
                Author        = book.Author,
                Publisher     = book.Publisher,
                PublishedYear = book.PublishedYear,
                CategoryId    = book.CategoryId,
                Edition       = book.Edition,
                Language      = book.Language,
                ShelfLocation = book.ShelfLocation,
                Description   = book.Description,
                BookPrice     = book.BookPrice,
                IsActive      = book.IsActive
            };

            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId", "CategoryName", book.CategoryId);

            return View(dto);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookDto dto)
        {
            if (id != dto.BookId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _categoryService.GetAllCategoriesAsync(),
                    "CategoryId", "CategoryName", dto.CategoryId);
                return View(dto);
            }

            var userId = GetCurrentUserId();
            var (success, message) = await _bookService.UpdateBookAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = message;
            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId", "CategoryName", dto.CategoryId);
            return View(dto);
        }

        // ============================================================
        // BOOK COPIES & QR CODES
        // ============================================================
        [HttpGet("Copies/{id}")]
        public async Task<IActionResult> Copies(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var copies = await _bookService.GetBookCopiesAsync(id);

            ViewBag.Book = book;
            return View(copies);
        }

        // ============================================================
        // ADD MORE COPIES
        // ============================================================
        [HttpGet("AddCopies/{id}")]
        public async Task<IActionResult> AddCopies(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            var dto = new AddCopiesDto
            {
                BookId    = book.BookId,
                BookTitle = book.Title,
                CopyPrice = book.BookPrice
            };

            return View(dto);
        }

        [HttpPost("AddCopies/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCopies(int id, AddCopiesDto dto)
        {
            if (id != dto.BookId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            var (success, message) = await _bookService.AddCopiesAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Copies), new { id = dto.BookId });
            }

            TempData["Error"] = message;
            return View(dto);
        }

        // ============================================================
        // QR CODE IMAGE
        // ============================================================
        [HttpGet("QRCode/{accessionNo}")]
        [AllowAnonymous]
        public async Task<IActionResult> QRCode(string accessionNo)
        {
            var imageBytes = await _bookService.GetQRCodeImageAsync(accessionNo);
            if (imageBytes == null)
                return NotFound();

            return File(imageBytes, "image/png");
        }

        // ============================================================
        // SCAN INFO (Public or Internal)
        // ============================================================
        [HttpGet("Scan/{accessionNo}")]
        [AllowAnonymous]
        public async Task<IActionResult> Scan(string accessionNo)
        {
            var info = await _bookService.GetBookScanInfoAsync(accessionNo);
            if (info == null)
                return NotFound("Book copy not found.");

            return View(info);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }
    }
}
