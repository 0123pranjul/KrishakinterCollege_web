using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    // ============================================================
    // BOOK CATEGORY REPOSITORY
    // ============================================================
    public interface IBookCategoryRepository
    {
        Task<List<LibBookCategory>> GetAllAsync(bool activeOnly = true);
        Task<LibBookCategory?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string name, int excludeId = 0);
        Task<LibBookCategory> CreateAsync(LibBookCategory category);
        Task<LibBookCategory> UpdateAsync(LibBookCategory category);
        Task<bool> DeleteAsync(int id);
    }

    // ============================================================
    // BOOK REPOSITORY
    // ============================================================
    public interface IBookRepository
    {
        Task<(List<BookListItemDto> Items, int TotalCount)> SearchAsync(BookSearchDto filter);
        Task<LibBook?> GetByIdAsync(int id);
        Task<LibBook?> GetByIdWithCopiesAsync(int id);
        Task<LibBook> CreateAsync(LibBook book);
        Task<LibBook> UpdateAsync(LibBook book);
        Task<bool> DeactivateAsync(int id, int updatedBy);
        Task<List<LibBookCopy>> GetCopiesAsync(int bookId);
        Task<LibBookCopy?> GetCopyByAccessionAsync(string accessionNo);
        Task<string> GenerateAccessionNoAsync(string prefix, int year);
        Task<LibBookCopy> AddCopyAsync(LibBookCopy copy);
        Task<bool> UpdateCopyAvailabilityAsync(int copyId, bool isAvailable, string? condition = null);
        Task<bool> UpdateBookCountsAsync(int bookId, int totalDelta, int availableDelta);
    }

    // ============================================================
    // FINE POLICY REPOSITORY
    // ============================================================
    public interface IFinePolicyRepository
    {
        Task<LibFinePolicy?> GetActivePolicyAsync();
        Task<LibFinePolicy?> GetByIdAsync(int id);
        Task<List<LibFinePolicy>> GetAllAsync();
        Task<LibFinePolicy> CreateAsync(LibFinePolicy policy);
        Task DeactivateAllAsync();
    }

    // ============================================================
    // ISSUE TRANSACTION REPOSITORY
    // ============================================================
    public interface IIssueTransactionRepository
    {
        Task<LibIssueTransaction?> GetByIdAsync(int issueId);
        Task<LibIssueTransaction?> GetOpenIssueByAccessionAsync(string accessionNo);
        Task<List<LibIssueTransaction>> GetMemberCurrentIssuesAsync(string userType, int userId);
        Task<int> GetMemberCurrentIssueCountAsync(string userType, int userId);
        Task<List<OverdueItemDto>> GetOverdueListAsync();
        Task<List<IssueTransactionDto>> GetTodayIssuedAsync();
        Task<List<IssueTransactionDto>> GetTodayReturnedAsync();
        Task<List<IssueHistoryDto>> GetMemberHistoryAsync(string userType, int userId, int page, int pageSize);
        Task<LibIssueTransaction> CreateAsync(LibIssueTransaction transaction);
        Task<LibIssueTransaction> UpdateAsync(LibIssueTransaction transaction);
        Task<bool> HasPendingFineAsync(string userType, int userId);
        Task<decimal> GetPendingFineAmountAsync(string userType, int userId);
    }

    // ============================================================
    // MEMBER BLOCK REPOSITORY
    // ============================================================
    public interface IMemberBlockRepository
    {
        Task<bool> IsBlockedAsync(string userType, int userId);
        Task<LibMemberBlockLog?> GetActiveBlockAsync(string userType, int userId);
        Task<LibMemberBlockLog> BlockMemberAsync(LibMemberBlockLog blockLog);
        Task<bool> UnblockMemberAsync(string userType, int userId, int unblockedBy, string reason);
    }

    // ============================================================
    // FINE PAYMENT REPOSITORY
    // ============================================================
    public interface IFinePaymentRepository
    {
        Task<decimal> GetTotalPaidAsync(int issueId);
        Task<List<LibFinePayment>> GetPaymentsByIssueAsync(int issueId);
        Task<LibFinePayment> CreateAsync(LibFinePayment payment);
        Task<decimal> GetTodayCollectionAsync();
        Task<List<MonthlyFineDto>> GetMonthlyCollectionAsync(int months);
    }

    // ============================================================
    // LIBRARY SETTINGS REPOSITORY
    // ============================================================
    public interface ILibSettingsRepository
    {
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value, int updatedBy);
    }
}
