using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace School_CRM.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueTransactionRepository _issueRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IFinePolicyService _policyService;
        private readonly IMemberBlockRepository _blockRepo;
        private readonly IFinePaymentRepository _fineRepo;
        private readonly ILibraryMemberService _memberService;
        private readonly LibmanagementContext _context;

        public IssueService(
            IIssueTransactionRepository issueRepo,
            IBookRepository bookRepo,
            IFinePolicyService policyService,
            IMemberBlockRepository blockRepo,
            IFinePaymentRepository fineRepo,
            ILibraryMemberService memberService,
            LibmanagementContext context)
        {
            _issueRepo     = issueRepo;
            _bookRepo      = bookRepo;
            _policyService = policyService;
            _blockRepo     = blockRepo;
            _fineRepo      = fineRepo;
            _memberService = memberService;
            _context       = context;
        }

        public async Task<MemberEligibilityDto> CheckMemberEligibilityAsync(string userType, int userId)
        {
            var result = new MemberEligibilityDto();
            var policy = await _policyService.GetActivePolicyAsync();

            if (policy == null)
            {
                result.Messages.Add("No active fine policy found. Contact administrator.");
                return result;
            }

            // Check block status
            var isBlocked = await _blockRepo.IsBlockedAsync(userType, userId);
            if (isBlocked)
            {
                var block = await _blockRepo.GetActiveBlockAsync(userType, userId);
                result.IsBlocked  = true;
                result.BlockReason = block?.BlockReason ?? "Account is blocked";
                result.Messages.Add($"Member is blocked: {result.BlockReason}");
            }

            // Check pending fine
            var hasPendingFine = await _issueRepo.HasPendingFineAsync(userType, userId);
            if (hasPendingFine)
            {
                result.HasPendingFine      = true;
                result.PendingFineAmount   = await _issueRepo.GetPendingFineAmountAsync(userType, userId);
                result.Messages.Add($"Pending fine of ₹{result.PendingFineAmount:F2} exists. Please clear before issuing.");
            }

            // Check issue limit
            int currentCount = await _issueRepo.GetMemberCurrentIssueCountAsync(userType, userId);
            int maxAllowed   = userType == "Student" ? policy.MaxBooksForStudent : policy.MaxBooksForTeacher;

            result.CurrentIssuedCount = currentCount;
            result.MaxAllowed         = maxAllowed;

            if (currentCount >= maxAllowed)
            {
                result.LimitReached = true;
                result.Messages.Add($"Issue limit reached. {currentCount}/{maxAllowed} books currently issued.");
            }

            result.IsEligible = !result.IsBlocked && !result.HasPendingFine && !result.LimitReached;
            return result;
        }

        public async Task<(bool Success, string Message, int IssueId)> IssueBookAsync(IssueBookDto dto)
        {
            var policy = await _policyService.GetActivePolicyAsync();
            if (policy == null)
                return (false, "No active fine policy found.", 0);

            // Validate member eligibility
            var eligibility = await CheckMemberEligibilityAsync(dto.UserType, dto.UserId);
            if (!eligibility.IsEligible)
                return (false, string.Join("; ", eligibility.Messages), 0);

            // Validate book copy
            var copy = await _bookRepo.GetCopyByAccessionAsync(dto.AccessionNo);
            if (copy == null)
                return (false, "Book copy not found.", 0);

            if (!copy.IsAvailable)
                return (false, "This copy is already issued.", 0);

            if (copy.CopyCondition is "Lost" or "Withdrawn")
                return (false, $"Cannot issue a book with condition: {copy.CopyCondition}.", 0);

            // Calculate due date
            int issueDays = dto.UserType == "Student"
                ? policy.IssueDaysForStudent
                : policy.IssueDaysForTeacher;

            var issueDate = DateOnly.FromDateTime(DateTime.Today);
            var dueDate   = issueDate.AddDays(issueDays);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create issue transaction
                var issue = new LibIssueTransaction
                {
                    UserType          = dto.UserType,
                    UserId            = dto.UserId,
                    CopyId            = copy.CopyId,
                    PolicyId          = policy.PolicyId,
                    IssuedBy          = dto.IssuedBy,
                    IssueDate         = issueDate,
                    DueDate           = dueDate,
                    IsReturned        = false,
                    FineAmount        = 0,
                    IsFinePaid        = false,
                    TransactionStatus = "Issued",
                    Remarks           = dto.Remarks,
                    CreatedAt         = DateTime.Now
                };

                await _issueRepo.CreateAsync(issue);

                // Update copy availability
                await _bookRepo.UpdateCopyAvailabilityAsync(copy.CopyId, false);

                // Update book available count
                await _bookRepo.UpdateBookCountsAsync(copy.BookId, 0, -1);

                await transaction.CommitAsync();

                return (true, $"Book issued successfully. Due Date: {dueDate:dd/MM/yyyy}", issue.IssueId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to issue book: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message, decimal FineAmount)> ReturnBookAsync(ReturnBookDto dto)
        {
            var issue = await _issueRepo.GetOpenIssueByAccessionAsync(dto.AccessionNo);
            if (issue == null)
                return (false, "No open issue found for this book.", 0);

            var policy = issue.Policy;
            var copy   = issue.Copy;

            // Calculate fines
            decimal overdueFine = _policyService.CalculateOverdueFine(policy, issue.DueDate);
            decimal totalFine   = overdueFine;
            string? fineType    = overdueFine > 0 ? "Overdue" : null;

            if (dto.ConditionOnReturn == "Damaged")
            {
                decimal damageFine = _policyService.CalculateDamageFine(policy, copy.CopyPrice);
                totalFine += damageFine;
                fineType   = overdueFine > 0 ? "Mixed" : "Damaged";
            }
            else if (dto.ConditionOnReturn == "Lost")
            {
                decimal lostFine = _policyService.CalculateLostFine(policy, copy.CopyPrice);
                totalFine += lostFine;
                fineType   = overdueFine > 0 ? "Mixed" : "Lost";
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            int overdueDays = Math.Max(0,
                (today.ToDateTime(TimeOnly.MinValue) - issue.DueDate.ToDateTime(TimeOnly.MinValue)).Days
                - policy.GracePeriodDays);

            string txnStatus = dto.ConditionOnReturn switch
            {
                "Damaged" => "Damaged",
                "Lost"    => "Lost",
                _         => "Returned"
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update issue transaction
                issue.ReturnDate        = today;
                issue.IsReturned        = true;
                issue.ReturnedTo        = dto.ReturnedTo;
                issue.ConditionOnReturn = dto.ConditionOnReturn;
                issue.OverdueDays       = overdueDays;
                issue.FineAmount        = totalFine;
                issue.FineType          = fineType;
                issue.TransactionStatus = txnStatus;
                issue.Remarks           = dto.Remarks;

                await _issueRepo.UpdateAsync(issue);

                // Update copy
                bool isAvailableAfterReturn = dto.ConditionOnReturn != "Lost";
                await _bookRepo.UpdateCopyAvailabilityAsync(copy.CopyId, isAvailableAfterReturn, dto.ConditionOnReturn);

                // Update book counts
                if (isAvailableAfterReturn)
                    await _bookRepo.UpdateBookCountsAsync(copy.BookId, 0, 1);

                // Block member if fine exists
                if (totalFine > 0)
                {
                    string blockType = dto.ConditionOnReturn switch
                    {
                        "Lost"    => "LostBook",
                        "Damaged" => "DamagedBook",
                        _         => "PendingFine"
                    };

                    await _blockRepo.BlockMemberAsync(new LibMemberBlockLog
                    {
                        UserType    = issue.UserType,
                        UserId      = issue.UserId,
                        BlockReason = $"Fine of ₹{totalFine:F2} pending ({txnStatus})",
                        BlockType   = blockType,
                        IssueId     = issue.IssueId,
                        IsBlocked   = true,
                        BlockedBy   = dto.ReturnedTo,
                        BlockedAt   = DateTime.Now
                    });
                }

                await transaction.CommitAsync();
                return (true, totalFine > 0
                    ? $"Book returned. Fine of ₹{totalFine:F2} is pending."
                    : "Book returned successfully.", totalFine);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to process return: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)> MarkBookLostAsync(MarkLostDto dto, int staffId)
        {
            var issue = await _issueRepo.GetByIdAsync(dto.IssueId);
            if (issue == null)
                return (false, "Issue transaction not found.");

            if (issue.IsReturned)
                return (false, "This book has already been returned.");

            var policy = issue.Policy;
            var copy   = issue.Copy;

            decimal overdueFine = _policyService.CalculateOverdueFine(policy, issue.DueDate);
            decimal lostFine    = _policyService.CalculateLostFine(policy, copy.CopyPrice);
            decimal totalFine   = overdueFine + lostFine;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                issue.TransactionStatus = "Lost";
                issue.FineAmount        = totalFine;
                issue.FineType          = overdueFine > 0 ? "Mixed" : "Lost";
                issue.ConditionOnReturn = "Lost";
                issue.Remarks           = dto.Remarks;

                await _issueRepo.UpdateAsync(issue);

                // Mark copy as lost
                await _bookRepo.UpdateCopyAvailabilityAsync(copy.CopyId, false, "Lost");

                // Reduce total copies (lost book is gone)
                await _bookRepo.UpdateBookCountsAsync(copy.BookId, -1, 0);

                // Block member
                await _blockRepo.BlockMemberAsync(new LibMemberBlockLog
                {
                    UserType    = issue.UserType,
                    UserId      = issue.UserId,
                    BlockReason = $"Book declared lost. Fine of ₹{totalFine:F2} pending.",
                    BlockType   = "LostBook",
                    IssueId     = issue.IssueId,
                    IsBlocked   = true,
                    BlockedBy   = staffId,
                    BlockedAt   = DateTime.Now
                });

                await transaction.CommitAsync();
                return (true, $"Book marked as lost. Fine of ₹{totalFine:F2} has been applied.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to mark book as lost: {ex.Message}");
            }
        }

        public async Task<List<OverdueItemDto>> GetOverdueBooksAsync()
        {
            return await _issueRepo.GetOverdueListAsync();
        }

        public async Task<List<IssueHistoryDto>> GetMemberHistoryAsync(string userType, int userId, int page = 1, int pageSize = 10)
        {
            return await _issueRepo.GetMemberHistoryAsync(userType, userId, page, pageSize);
        }

        public async Task<ReturnBookDto?> GetReturnInfoByAccessionAsync(string accessionNo)
        {
            var issue = await _issueRepo.GetOpenIssueByAccessionAsync(accessionNo);
            if (issue == null) return null;

            var policy = issue.Policy;
            decimal overdueFine = _policyService.CalculateOverdueFine(policy, issue.DueDate);

            var today = DateOnly.FromDateTime(DateTime.Today);
            int overdueDays = Math.Max(0,
                (today.ToDateTime(TimeOnly.MinValue) - issue.DueDate.ToDateTime(TimeOnly.MinValue)).Days
                - policy.GracePeriodDays);

            var member = await _memberService.GetMemberAsync(issue.UserType, issue.UserId);

            return new ReturnBookDto
            {
                AccessionNo      = accessionNo,
                IssueId          = issue.IssueId,
                MemberName       = member?.Name ?? $"{issue.UserType} #{issue.UserId}",
                BookTitle        = issue.Copy.Book.Title,
                Author           = issue.Copy.Book.Author,
                IssueDate        = issue.IssueDate,
                DueDate          = issue.DueDate,
                OverdueDays      = overdueDays,
                CalculatedFine   = overdueFine,
                ConditionOnReturn = "Good"
            };
        }
    }
}
