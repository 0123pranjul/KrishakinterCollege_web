using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class LibraryDashboardService : ILibraryDashboardService
    {
        private readonly IIssueTransactionRepository _issueRepo;
        private readonly IFinePaymentRepository _fineRepo;
        private readonly IMemberBlockRepository _blockRepo;
        private readonly IFinePolicyService _policyService;
        private readonly ILibraryMemberService _memberService;
        private readonly LibmanagementContext _context;

        public LibraryDashboardService(
            IIssueTransactionRepository issueRepo,
            IFinePaymentRepository fineRepo,
            IMemberBlockRepository blockRepo,
            IFinePolicyService policyService,
            ILibraryMemberService memberService,
            LibmanagementContext context)
        {
            _issueRepo     = issueRepo;
            _fineRepo      = fineRepo;
            _blockRepo     = blockRepo;
            _policyService = policyService;
            _memberService = memberService;
            _context       = context;
        }

        public async Task<LibrarianDashboardDto> GetLibrarianDashboardAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var issuedToday = await _context.LibIssueTransactions
                .CountAsync(x => x.IssueDate == today);

            var returnedToday = await _context.LibIssueTransactions
                .CountAsync(x => x.ReturnDate == today && x.IsReturned);

            var fineToday = await _fineRepo.GetTodayCollectionAsync();

            var overdueCount = await _context.LibIssueTransactions
                .CountAsync(x => !x.IsReturned && x.DueDate < today);

            var outOfStock = await _context.LibBooks
                .CountAsync(x => x.AvailableCopies == 0 && x.IsActive);

            var blockedCount = await _context.LibMemberBlockLogs
                .CountAsync(x => x.IsBlocked);

            var topOverdue = await _issueRepo.GetOverdueListAsync();
            var todayReturns = await _issueRepo.GetTodayReturnedAsync();
            var recentIssues = await _issueRepo.GetTodayIssuedAsync();
            var monthlyFine  = await _fineRepo.GetMonthlyCollectionAsync(6);

            // Blocked members list
            var blockedLogs = await _context.LibMemberBlockLogs
                .Where(x => x.IsBlocked)
                .OrderByDescending(x => x.BlockedAt)
                .Take(10)
                .ToListAsync();

            var blockedList = new List<BlockedMemberDto>();
            foreach (var log in blockedLogs)
            {
                var member = await _memberService.GetMemberAsync(log.UserType, log.UserId);
                blockedList.Add(new BlockedMemberDto
                {
                    MemberName  = member?.Name ?? $"{log.UserType} #{log.UserId}",
                    UserType    = log.UserType,
                    BlockType   = log.BlockType,
                    BlockReason = log.BlockReason,
                    BlockedAt   = log.BlockedAt
                });
            }

            return new LibrarianDashboardDto
            {
                BooksIssuedToday    = issuedToday,
                BooksReturnedToday  = returnedToday,
                FineCollectedToday  = fineToday,
                OverdueCount        = overdueCount,
                OutOfStockBooks     = outOfStock,
                BlockedMembers      = blockedCount,
                TopOverdueBooks     = topOverdue.Take(10).ToList(),
                TodayReturns        = todayReturns,
                RecentIssues        = recentIssues,
                BlockedMembersList  = blockedList,
                MonthlyFineChart    = monthlyFine
            };
        }

        public async Task<MemberDashboardDto> GetMemberDashboardAsync(string userType, int userId)
        {
            var policy = await _policyService.GetActivePolicyAsync();
            int maxAllowed = policy != null
                ? (userType == "Student" ? policy.MaxBooksForStudent : policy.MaxBooksForTeacher)
                : 0;

            var currentIssues = await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.UserType == userType && x.UserId == userId && !x.IsReturned)
                .OrderBy(x => x.DueDate)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var currentBooks = currentIssues.Select(x =>
            {
                int daysRemaining = (x.DueDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
                return new CurrentIssueDto
                {
                    IssueId       = x.IssueId,
                    BookTitle     = x.Copy.Book.Title,
                    Author        = x.Copy.Book.Author,
                    AccessionNo   = x.Copy.AccessionNo,
                    IssueDate     = x.IssueDate,
                    DueDate       = x.DueDate,
                    DaysRemaining = daysRemaining,
                    IsOverdue     = daysRemaining < 0,
                    Status        = daysRemaining < 0 ? "Overdue"
                                  : daysRemaining <= 3 ? "Due Soon"
                                  : "Active"
                };
            }).ToList();

            var isBlocked = await _blockRepo.IsBlockedAsync(userType, userId);
            var block     = isBlocked ? await _blockRepo.GetActiveBlockAsync(userType, userId) : null;

            var pendingFine = await _context.LibIssueTransactions
                .Where(x => x.UserType == userType && x.UserId == userId
                         && !x.IsFinePaid && x.FineAmount > 0)
                .SumAsync(x => (decimal?)x.FineAmount) ?? 0;

            var history = await _context.LibIssueTransactions
                .Include(x => x.Copy)
                .ThenInclude(c => c.Book)
                .Where(x => x.UserType == userType && x.UserId == userId && x.IsReturned)
                .OrderByDescending(x => x.ReturnDate)
                .Take(10)
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

            return new MemberDashboardDto
            {
                CurrentIssuedCount = currentIssues.Count,
                MaxAllowed         = maxAllowed,
                RemainingLimit     = Math.Max(0, maxAllowed - currentIssues.Count),
                IsBlocked          = isBlocked,
                BlockReason        = block?.BlockReason,
                PendingFine        = pendingFine,
                CurrentBooks       = currentBooks,
                IssueHistory       = history
            };
        }
    }
}
