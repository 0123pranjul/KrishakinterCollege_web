using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    // ============================================================
    // BOOK CATEGORY SERVICE
    // ============================================================
    public interface IBookCategoryService
    {
        Task<List<LibBookCategory>> GetAllCategoriesAsync(bool activeOnly = true);
        Task<LibBookCategory?> GetCategoryByIdAsync(int id);
        Task<(bool Success, string Message)> CreateCategoryAsync(BookCategoryDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateCategoryAsync(BookCategoryDto dto, int updatedBy);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
    }

    // ============================================================
    // BOOK SERVICE
    // ============================================================
    public interface IBookService
    {
        Task<(List<BookListItemDto> Items, int TotalCount)> SearchBooksAsync(BookSearchDto filter);
        Task<LibBook?> GetBookByIdAsync(int id);
        Task<(bool Success, string Message, int BookId)> CreateBookAsync(BookDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateBookAsync(BookDto dto, int updatedBy);
        Task<(bool Success, string Message)> AddCopiesAsync(AddCopiesDto dto, int createdBy);
        Task<List<LibBookCopy>> GetBookCopiesAsync(int bookId);
        Task<BookScanInfoDto?> GetBookScanInfoAsync(string accessionNo);
        Task<byte[]?> GetQRCodeImageAsync(string accessionNo);
    }

    // ============================================================
    // FINE POLICY SERVICE
    // ============================================================
    public interface IFinePolicyService
    {
        Task<LibFinePolicy?> GetActivePolicyAsync();
        Task<List<LibFinePolicy>> GetAllPoliciesAsync();
        Task<(bool Success, string Message)> CreatePolicyAsync(FinePolicyDto dto, int createdBy);
        decimal CalculateOverdueFine(LibFinePolicy policy, DateOnly dueDate);
        decimal CalculateDamageFine(LibFinePolicy policy, decimal copyPrice);
        decimal CalculateLostFine(LibFinePolicy policy, decimal copyPrice);
    }

    // ============================================================
    // ISSUE SERVICE
    // ============================================================
    public interface IIssueService
    {
        Task<MemberEligibilityDto> CheckMemberEligibilityAsync(string userType, int userId);
        Task<(bool Success, string Message, int IssueId)> IssueBookAsync(IssueBookDto dto);
        Task<(bool Success, string Message, decimal FineAmount)> ReturnBookAsync(ReturnBookDto dto);
        Task<(bool Success, string Message)> MarkBookLostAsync(MarkLostDto dto, int staffId);
        Task<List<OverdueItemDto>> GetOverdueBooksAsync();
        Task<List<IssueHistoryDto>> GetMemberHistoryAsync(string userType, int userId, int page = 1, int pageSize = 10);
        Task<ReturnBookDto?> GetReturnInfoByAccessionAsync(string accessionNo);
    }

    // ============================================================
    // FINE PAYMENT SERVICE
    // ============================================================
    public interface IFinePaymentService
    {
        Task<FinePaymentDto?> GetFineDetailsAsync(int issueId);
        Task<(bool Success, string Message, string? ReceiptNo)> CollectFineAsync(FinePaymentDto dto);
    }

    // ============================================================
    // DASHBOARD SERVICE
    // ============================================================
    public interface ILibraryDashboardService
    {
        Task<LibrarianDashboardDto> GetLibrarianDashboardAsync();
        Task<MemberDashboardDto> GetMemberDashboardAsync(string userType, int userId);
    }

    // ============================================================
    // MEMBER LOOKUP SERVICE
    // ============================================================
    public interface ILibraryMemberService
    {
        Task<List<MemberLookupDto>> GetStudentsAsync(string? search = null);
        Task<List<MemberLookupDto>> GetTeachersAsync(string? search = null);
        Task<MemberLookupDto?> GetMemberAsync(string userType, int userId);
    }

    public class MemberLookupDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Photo { get; set; }
        public string UserType { get; set; } = null!;
        public string? ClassName { get; set; }
    }
}
