using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly LibmanagementContext _context;

        public BookRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<(List<BookListItemDto> Items, int TotalCount)> SearchAsync(BookSearchDto filter)
        {
            var query = _context.LibBooks
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim().ToLower();
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search) ||
                    x.Author.ToLower().Contains(search) ||
                    (x.Isbn != null && x.Isbn.Contains(search)) ||
                    (x.Publisher != null && x.Publisher.ToLower().Contains(search)));
            }

            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
                query = query.Where(x => x.CategoryId == filter.CategoryId);

            if (filter.OnlyAvailable)
                query = query.Where(x => x.AvailableCopies > 0);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Title)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new BookListItemDto
                {
                    BookId         = x.BookId,
                    ISBN           = x.Isbn,
                    Title          = x.Title,
                    Author         = x.Author,
                    Publisher      = x.Publisher,
                    CategoryName   = x.Category.CategoryName,
                    ShelfLocation  = x.ShelfLocation,
                    TotalCopies    = x.TotalCopies,
                    AvailableCopies = x.AvailableCopies,
                    BookPrice      = x.BookPrice,
                    IsActive       = x.IsActive
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<LibBook?> GetByIdAsync(int id)
        {
            return await _context.LibBooks
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.BookId == id);
        }

        public async Task<LibBook?> GetByIdWithCopiesAsync(int id)
        {
            return await _context.LibBooks
                .Include(x => x.Category)
                .Include(x => x.LibBookCopies)
                .FirstOrDefaultAsync(x => x.BookId == id);
        }

        public async Task<LibBook> CreateAsync(LibBook book)
        {
            _context.LibBooks.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<LibBook> UpdateAsync(LibBook book)
        {
            _context.LibBooks.Update(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<bool> DeactivateAsync(int id, int updatedBy)
        {
            var book = await GetByIdAsync(id);
            if (book == null) return false;

            book.IsActive  = false;
            book.UpdatedAt = DateTime.Now;
            book.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LibBookCopy>> GetCopiesAsync(int bookId)
        {
            return await _context.LibBookCopies
                .Where(x => x.BookId == bookId && x.IsActive)
                .OrderBy(x => x.AccessionNo)
                .ToListAsync();
        }

        public async Task<LibBookCopy?> GetCopyByAccessionAsync(string accessionNo)
        {
            return await _context.LibBookCopies
                .Include(x => x.Book)
                .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(x => x.AccessionNo == accessionNo && x.IsActive);
        }

        public async Task<string> GenerateAccessionNoAsync(string prefix, int year)
        {
            var yearPrefix = $"{prefix}-{year}-";
            var count = await _context.LibBookCopies
                .CountAsync(x => x.AccessionNo.StartsWith(yearPrefix));
            
            return $"{yearPrefix}{(count + 1):D5}";
        }

        public async Task<LibBookCopy> AddCopyAsync(LibBookCopy copy)
        {
            _context.LibBookCopies.Add(copy);
            await _context.SaveChangesAsync();
            return copy;
        }

        public async Task<bool> UpdateCopyAvailabilityAsync(int copyId, bool isAvailable, string? condition = null)
        {
            var copy = await _context.LibBookCopies.FindAsync(copyId);
            if (copy == null) return false;

            copy.IsAvailable = isAvailable;
            if (condition != null)
                copy.CopyCondition = condition;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBookCountsAsync(int bookId, int totalDelta, int availableDelta)
        {
            var book = await _context.LibBooks.FindAsync(bookId);
            if (book == null) return false;

            book.TotalCopies     = Math.Max(0, book.TotalCopies + totalDelta);
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + availableDelta);
            book.UpdatedAt       = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
