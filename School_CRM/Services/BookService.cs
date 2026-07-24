using System.Text.Json;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly ILibSettingsRepository _settingsRepo;
        private readonly QRCodeService _qrService;
        private readonly IWebHostEnvironment _env;

        public BookService(
            IBookRepository bookRepo,
            ILibSettingsRepository settingsRepo,
            QRCodeService qrService,
            IWebHostEnvironment env)
        {
            _bookRepo     = bookRepo;
            _settingsRepo = settingsRepo;
            _qrService    = qrService;
            _env          = env;
        }

        public async Task<(List<BookListItemDto> Items, int TotalCount)> SearchBooksAsync(BookSearchDto filter)
        {
            return await _bookRepo.SearchAsync(filter);
        }

        public async Task<LibBook?> GetBookByIdAsync(int id)
        {
            return await _bookRepo.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, int BookId)> CreateBookAsync(BookDto dto, int createdBy)
        {
            var prefix = await _settingsRepo.GetValueAsync("AccessionPrefix") ?? "LIB";
            var baseUrl = await _settingsRepo.GetValueAsync("QRCodeBaseURL") ?? "https://localhost/library/book/scan/";
            var year = DateTime.Today.Year;

            // Create book master record
            var book = new LibBook
            {
                Isbn           = dto.ISBN?.Trim(),
                Title          = dto.Title.Trim(),
                Author         = dto.Author.Trim(),
                Publisher      = dto.Publisher?.Trim(),
                PublishedYear  = dto.PublishedYear,
                CategoryId     = dto.CategoryId,
                Edition        = dto.Edition?.Trim(),
                Language       = dto.Language,
                ShelfLocation  = dto.ShelfLocation?.Trim(),
                Description    = dto.Description?.Trim(),
                BookPrice      = dto.BookPrice,
                TotalCopies    = dto.NumberOfCopies ?? 1,
                AvailableCopies = dto.NumberOfCopies ?? 1,
                IsActive       = true,
                CreatedAt      = DateTime.Now,
                CreatedBy      = createdBy
            };

            await _bookRepo.CreateAsync(book);

            // Create copies
            int copies = dto.NumberOfCopies ?? 1;
            for (int i = 0; i < copies; i++)
            {
                var accessionNo = await _bookRepo.GenerateAccessionNoAsync(prefix, year);

                var qrData = new QRCodeDataDto
                {
                    CopyId        = 0, // will update after save
                    AccessionNo   = accessionNo,
                    Title         = book.Title,
                    Author        = book.Author,
                    ISBN          = book.Isbn,
                    ShelfLocation = book.ShelfLocation,
                    ScanURL       = $"{baseUrl}{accessionNo}"
                };

                var copy = new LibBookCopy
                {
                    BookId          = book.BookId,
                    AccessionNo     = accessionNo,
                    QrcodeData      = JsonSerializer.Serialize(qrData),
                    CopyCondition   = "Good",
                    IsAvailable     = true,
                    AcquisitionDate = DateOnly.FromDateTime(DateTime.Today),
                    CopyPrice       = dto.BookPrice,
                    IsActive        = true,
                    CreatedAt       = DateTime.Now,
                    CreatedBy       = createdBy
                };

                await _bookRepo.AddCopyAsync(copy);

                // Update QR data with actual CopyId and generate image
                qrData.CopyId = copy.CopyId;
                var imagePath = _qrService.GenerateQRCode(qrData);

                copy.QrcodeData      = JsonSerializer.Serialize(qrData);
                copy.QrcodeImagePath = imagePath;
                await _bookRepo.AddCopyAsync(copy); // update
            }

            return (true, "Book added successfully.", book.BookId);
        }

        public async Task<(bool Success, string Message)> UpdateBookAsync(BookDto dto, int updatedBy)
        {
            var book = await _bookRepo.GetByIdAsync(dto.BookId);
            if (book == null)
                return (false, "Book not found.");

            book.Isbn          = dto.ISBN?.Trim();
            book.Title         = dto.Title.Trim();
            book.Author        = dto.Author.Trim();
            book.Publisher     = dto.Publisher?.Trim();
            book.PublishedYear = dto.PublishedYear;
            book.CategoryId    = dto.CategoryId;
            book.Edition       = dto.Edition?.Trim();
            book.Language      = dto.Language;
            book.ShelfLocation = dto.ShelfLocation?.Trim();
            book.Description   = dto.Description?.Trim();
            book.BookPrice     = dto.BookPrice;
            book.UpdatedAt     = DateTime.Now;
            book.UpdatedBy     = updatedBy;

            await _bookRepo.UpdateAsync(book);
            return (true, "Book updated successfully.");
        }

        public async Task<(bool Success, string Message)> AddCopiesAsync(AddCopiesDto dto, int createdBy)
        {
            var book = await _bookRepo.GetByIdAsync(dto.BookId);
            if (book == null)
                return (false, "Book not found.");

            var prefix  = await _settingsRepo.GetValueAsync("AccessionPrefix") ?? "LIB";
            var baseUrl = await _settingsRepo.GetValueAsync("QRCodeBaseURL") ?? "https://localhost/library/book/scan/";
            var year    = DateTime.Today.Year;

            for (int i = 0; i < dto.NumberOfCopies; i++)
            {
                var accessionNo = await _bookRepo.GenerateAccessionNoAsync(prefix, year);

                var qrData = new QRCodeDataDto
                {
                    CopyId        = 0,
                    AccessionNo   = accessionNo,
                    Title         = book.Title,
                    Author        = book.Author,
                    ISBN          = book.Isbn,
                    ShelfLocation = book.ShelfLocation,
                    ScanURL       = $"{baseUrl}{accessionNo}"
                };

                var copy = new LibBookCopy
                {
                    BookId          = book.BookId,
                    AccessionNo     = accessionNo,
                    QrcodeData      = JsonSerializer.Serialize(qrData),
                    CopyCondition   = "Good",
                    IsAvailable     = true,
                    AcquisitionDate = dto.AcquisitionDate,
                    CopyPrice       = dto.CopyPrice,
                    IsActive        = true,
                    CreatedAt       = DateTime.Now,
                    CreatedBy       = createdBy
                };

                await _bookRepo.AddCopyAsync(copy);

                qrData.CopyId = copy.CopyId;
                var imagePath = _qrService.GenerateQRCode(qrData);
                copy.QrcodeData      = JsonSerializer.Serialize(qrData);
                copy.QrcodeImagePath = imagePath;
                await _bookRepo.AddCopyAsync(copy);
            }

            // Update book counts
            await _bookRepo.UpdateBookCountsAsync(book.BookId, dto.NumberOfCopies, dto.NumberOfCopies);

            return (true, $"{dto.NumberOfCopies} copies added successfully.");
        }

        public async Task<List<LibBookCopy>> GetBookCopiesAsync(int bookId)
        {
            return await _bookRepo.GetCopiesAsync(bookId);
        }

        public async Task<BookScanInfoDto?> GetBookScanInfoAsync(string accessionNo)
        {
            var copy = await _bookRepo.GetCopyByAccessionAsync(accessionNo);
            if (copy == null) return null;

            DateOnly? dueDate = null;
            if (!copy.IsAvailable)
            {
                // Find current issue
                // This is handled in the controller via IssueService
            }

            return new BookScanInfoDto
            {
                CopyId        = copy.CopyId,
                AccessionNo   = copy.AccessionNo,
                Title         = copy.Book.Title,
                Author        = copy.Book.Author,
                Publisher     = copy.Book.Publisher,
                CategoryName  = copy.Book.Category.CategoryName,
                ShelfLocation = copy.Book.ShelfLocation,
                CopyCondition = copy.CopyCondition,
                IsAvailable   = copy.IsAvailable,
                DueDate       = dueDate
            };
        }

        public async Task<byte[]?> GetQRCodeImageAsync(string accessionNo)
        {
            return _qrService.GetQRCodeImage(accessionNo);
        }
    }
}
