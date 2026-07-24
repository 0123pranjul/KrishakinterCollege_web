using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class IssueTransactionRepository : IIssueTransactionRepository
    {
        private readonly LibmanagementContext _context;

        public IssueTransactionRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<LibIssueTransaction?> GetByIdAsync(int issueId)
        {
            return await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .ThenInclude(b => b.Category)
                .Include(x => x.Policy)
                .FirstOrDefaultAsync(x => x.IssueId == issueId);
        }

        public async Task<LibIssueTransaction?> GetOpenIssueByAccessionAsync(string accessionNo)
        {
            return await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Include(x => x.Policy)
                .Where(x => x.Copy.AccessionNo == accessionNo && !x.IsReturned)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LibIssueTransaction>> GetMemberCurrentIssuesAsync(string userType, int userId)
        {
            return await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.UserType == userType && x.UserId == userId && !x.IsReturned)
                .OrderBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<int> GetMemberCurrentIssueCountAsync(string userType, int userId)
        {
            return await _context.LibIssueTransactions
                .CountAsync(x => x.UserType == userType && x.UserId == userId && !x.IsReturned);
        }

        public async Task<List<OverdueItemDto>> GetOverdueListAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var overdueItems = await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Include(x => x.Policy)
                .Where(x => !x.IsReturned && x.DueDate < today)
                .OrderBy(x => x.DueDate)
                .ToListAsync();

            var result = new List<OverdueItemDto>();

            foreach (var item in overdueItems)
            {
                var memberName = await GetMemberNameAsync(item.UserType, item.UserId);
                var overdueDays = (today.ToDateTime(TimeOnly.MinValue) - item.DueDate.ToDateTime(TimeOnly.MinValue)).Days;
                var estimatedFine = Math.Min(
                    overdueDays * item.Policy.PerDayFine,
                    item.Policy.MaxOverdueFine ?? decimal.MaxValue);

                result.Add(new OverdueItemDto
                {
                    IssueId       = item.IssueId,
                    MemberName    = memberName,
                    UserType      = item.UserType,
                    BookTitle     = item.Copy.Book.Title,
                    DueDate       = item.DueDate,
                    OverdueDays   = overdueDays,
                    EstimatedFine = estimatedFine
                });
            }

            return result;
        }

        public async Task<List<IssueTransactionDto>> GetTodayIssuedAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var items = await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.IssueDate == today)
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .ToListAsync();

            var result = new List<IssueTransactionDto>();
            foreach (var item in items)
            {
                result.Add(new IssueTransactionDto
                {
                    IssueId           = item.IssueId,
                    MemberName        = await GetMemberNameAsync(item.UserType, item.UserId),
                    BookTitle         = item.Copy.Book.Title,
                    IssueDate         = item.IssueDate,
                    DueDate           = item.DueDate,
                    TransactionStatus = item.TransactionStatus
                });
            }
            return result;
        }

        public async Task<List<IssueTransactionDto>> GetTodayReturnedAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var items = await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.ReturnDate == today && x.IsReturned)
                .OrderByDescending(x => x.CreatedAt)
                .Take(20)
                .ToListAsync();

            var result = new List<IssueTransactionDto>();
            foreach (var item in items)
            {
                result.Add(new IssueTransactionDto
                {
                    IssueId             = item.IssueId,
                    MemberName          = await GetMemberNameAsync(item.UserType, item.UserId),
                    BookTitle           = item.Copy.Book.Title,
                    IssueDate           = item.IssueDate,
                    DueDate             = item.DueDate,
                    ReturnDate          = item.ReturnDate,
                    FineAmount          = item.FineAmount,
                    ConditionOnReturn   = item.ConditionOnReturn,
                    TransactionStatus   = item.TransactionStatus
                });
            }
            return result;
        }

        public async Task<List<IssueHistoryDto>> GetMemberHistoryAsync(string userType, int userId, int page, int pageSize)
        {
            return await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.UserType == userType && x.UserId == userId && x.IsReturned)
                .OrderByDescending(x => x.ReturnDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IssueHistoryDto
                {
                    IssueId           = x.IssueId,
                    BookTitle         = x.Copy.Book.Title,
                    IssueDate         = x.IssueDate,
                    ReturnDate        = x.ReturnDate,
                    FineAmount        = x.FineAmount,
                    IsFinePaid        = x.IsFinePaid,
                    TransactionStatus = x.TransactionStatus
                })
                .ToListAsync();
        }

        public async Task<LibIssueTransaction> CreateAsync(LibIssueTransaction transaction)
        {
            _context.LibIssueTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<LibIssueTransaction> UpdateAsync(LibIssueTransaction transaction)
        {
            _context.LibIssueTransactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> HasPendingFineAsync(string userType, int userId)
        {
            return await _context.LibIssueTransactions
                .AnyAsync(x => x.UserType == userType
                            && x.UserId == userId
                            && x.IsReturned
                            && x.FineAmount > 0
                            && !x.IsFinePaid);
        }

        public async Task<decimal> GetPendingFineAmountAsync(string userType, int userId)
        {
            return await _context.LibIssueTransactions
                .Where(x => x.UserType == userType
                         && x.UserId == userId
                         && !x.IsFinePaid
                         && x.FineAmount > 0)
                .SumAsync(x => x.FineAmount);
        }

        private async Task<string> GetMemberNameAsync(string userType, int userId)
        {
            if (userType == "Student")
            {
                var student = await _context.TblStudents
                    .Where(s => s.StudentId == userId)
                    .Select(s => s.StudentName)
                    .FirstOrDefaultAsync();
                return student ?? $"Student #{userId}";
            }
            else
            {
                var teacher = await _context.TblTeachers
                    .Where(t => t.TeacherId == userId)
                    .Select(t => t.TeacherName)
                    .FirstOrDefaultAsync();
                return teacher ?? $"Teacher #{userId}";
            }
        }
    }
}
