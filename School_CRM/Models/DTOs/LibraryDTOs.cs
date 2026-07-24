using System.ComponentModel.DataAnnotations;

namespace School_CRM.Models.DTOs
{
    // ============================================================
    // BOOK CATEGORY DTOs
    // ============================================================
    public class BookCategoryDto
    {
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = null!;
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public int TotalBooks { get; set; }
    }

    // ============================================================
    // BOOK DTOs
    // ============================================================
    public class BookDto
    {
        public int BookId { get; set; }
        
        [StringLength(20)]
        public string? ISBN { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300)]
        public string Title { get; set; } = null!;
        
        [Required(ErrorMessage = "Author is required")]
        [StringLength(200)]
        public string Author { get; set; } = null!;
        
        [StringLength(200)]
        public string? Publisher { get; set; }
        
        public short? PublishedYear { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        
        public string? CategoryName { get; set; }
        
        [StringLength(50)]
        public string? Edition { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Language { get; set; } = "Hindi";
        
        [StringLength(50)]
        public string? ShelfLocation { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Book price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal BookPrice { get; set; }
        
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsActive { get; set; } = true;
        
        // For Add Book Form
        [Range(1, 100, ErrorMessage = "Number of copies must be between 1 and 100")]
        public int? NumberOfCopies { get; set; }
    }

    public class BookSearchDto
    {
        public string? SearchText { get; set; }
        public int? CategoryId { get; set; }
        public bool OnlyAvailable { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BookListItemDto
    {
        public int BookId { get; set; }
        public string? ISBN { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? Publisher { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? ShelfLocation { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public decimal BookPrice { get; set; }
        public bool IsActive { get; set; }
    }

    // ============================================================
    // BOOK COPY DTOs
    // ============================================================
    public class BookCopyDto
    {
        public int CopyId { get; set; }
        public int BookId { get; set; }
        public string AccessionNo { get; set; } = null!;
        public string? QRCodeData { get; set; }
        public string? QRCodeImagePath { get; set; }
        public string CopyCondition { get; set; } = "Good";
        public bool IsAvailable { get; set; }
        public DateOnly AcquisitionDate { get; set; }
        public decimal CopyPrice { get; set; }
        public string? Remarks { get; set; }
        
        // Navigation
        public string? BookTitle { get; set; }
        public string? Author { get; set; }
    }

    public class AddCopiesDto
    {
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        
        [Required]
        [Range(1, 100, ErrorMessage = "Number of copies must be between 1 and 100")]
        public int NumberOfCopies { get; set; }
        
        public DateOnly AcquisitionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        
        [Required]
        [Range(0.01, 999999.99)]
        public decimal CopyPrice { get; set; }
    }

    // ============================================================
    // FINE POLICY DTOs
    // ============================================================
    public class FinePolicyDto
    {
        public int PolicyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PolicyName { get; set; } = null!;
        
        [Required]
        [Range(0, 1000)]
        public decimal PerDayFine { get; set; } = 1.00m;
        
        [Range(0, 30)]
        public int GracePeriodDays { get; set; } = 0;
        
        [Range(0, 10000)]
        public decimal? MaxOverdueFine { get; set; }
        
        [Required]
        [Range(1, 20)]
        public int MaxBooksForStudent { get; set; } = 2;
        
        [Required]
        [Range(1, 50)]
        public int MaxBooksForTeacher { get; set; } = 5;
        
        [Required]
        [Range(1, 365)]
        public int IssueDaysForStudent { get; set; } = 14;
        
        [Required]
        [Range(1, 365)]
        public int IssueDaysForTeacher { get; set; } = 30;
        
        [Required]
        public string DamageFineType { get; set; } = "Percentage";
        
        [Required]
        [Range(0, 10000)]
        public decimal DamageFineValue { get; set; } = 50.00m;
        
        [Required]
        public string LostFineType { get; set; } = "BookPrice";
        
        [Required]
        [Range(0, 100)]
        public decimal LostFineValue { get; set; } = 1.00m;
        
        public bool IsActive { get; set; } = true;
    }

    // ============================================================
    // ISSUE TRANSACTION DTOs
    // ============================================================
    public class IssueBookDto
    {
        [Required]
        public string UserType { get; set; } = null!; // Student / Teacher
        
        [Required]
        public int UserId { get; set; }
        
        public string? MemberName { get; set; }
        public string? MemberCode { get; set; }
        public string? MemberPhoto { get; set; }
        
        [Required]
        public string AccessionNo { get; set; } = null!;
        
        public string? BookTitle { get; set; }
        public string? Author { get; set; }
        public string? CopyCondition { get; set; }
        
        public int IssuedBy { get; set; }
        public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly DueDate { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class ReturnBookDto
    {
        [Required]
        public string AccessionNo { get; set; } = null!;
        
        public int IssueId { get; set; }
        public string? MemberName { get; set; }
        public string? BookTitle { get; set; }
        public string? Author { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public int OverdueDays { get; set; }
        public decimal CalculatedFine { get; set; }
        
        [Required]
        public string ConditionOnReturn { get; set; } = "Good";
        
        public int ReturnedTo { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class MarkLostDto
    {
        public int IssueId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Remarks { get; set; } = null!;
        
        public DateOnly LostDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    }

    // ============================================================
    // FINE PAYMENT DTOs
    // ============================================================
    public class FinePaymentDto
    {
        public int PaymentId { get; set; }
        public int IssueId { get; set; }
        public string UserType { get; set; } = null!;
        public int UserId { get; set; }
        
        public string? MemberName { get; set; }
        public string? BookTitle { get; set; }
        public decimal TotalFine { get; set; }
        public decimal AlreadyPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        
        [Required]
        [Range(0.01, 999999.99)]
        public decimal AmountPaid { get; set; }
        
        [Required]
        public string PaymentMode { get; set; } = "Cash";
        
        [StringLength(50)]
        public string? ReceiptNo { get; set; }
        
        [StringLength(300)]
        public string? Remarks { get; set; }
        
        public int CollectedBy { get; set; }
    }

    // ============================================================
    // MEMBER ELIGIBILITY CHECK
    // ============================================================
    public class MemberEligibilityDto
    {
        public bool IsEligible { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public bool HasPendingFine { get; set; }
        public decimal PendingFineAmount { get; set; }
        public bool LimitReached { get; set; }
        public int CurrentIssuedCount { get; set; }
        public int MaxAllowed { get; set; }
        public List<string> Messages { get; set; } = new();
    }

    // ============================================================
    // DASHBOARD DTOs
    // ============================================================
    public class LibrarianDashboardDto
    {
        public int BooksIssuedToday { get; set; }
        public int BooksReturnedToday { get; set; }
        public decimal FineCollectedToday { get; set; }
        public int OverdueCount { get; set; }
        public int OutOfStockBooks { get; set; }
        public int BlockedMembers { get; set; }
        
        public List<OverdueItemDto> TopOverdueBooks { get; set; } = new();
        public List<IssueTransactionDto> TodayReturns { get; set; } = new();
        public List<IssueTransactionDto> RecentIssues { get; set; } = new();
        public List<BlockedMemberDto> BlockedMembersList { get; set; } = new();
        public List<MonthlyFineDto> MonthlyFineChart { get; set; } = new();
    }

    public class MemberDashboardDto
    {
        public int CurrentIssuedCount { get; set; }
        public int MaxAllowed { get; set; }
        public int RemainingLimit { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
        public decimal PendingFine { get; set; }
        
        public List<CurrentIssueDto> CurrentBooks { get; set; } = new();
        public List<IssueHistoryDto> IssueHistory { get; set; } = new();
    }

    public class OverdueItemDto
    {
        public int IssueId { get; set; }
        public string MemberName { get; set; } = null!;
        public string UserType { get; set; } = null!;
        public string BookTitle { get; set; } = null!;
        public DateOnly DueDate { get; set; }
        public int OverdueDays { get; set; }
        public decimal EstimatedFine { get; set; }
    }

    public class IssueTransactionDto
    {
        public int IssueId { get; set; }
        public string MemberName { get; set; } = null!;
        public string BookTitle { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public decimal FineAmount { get; set; }
        public string? ConditionOnReturn { get; set; }
        public string TransactionStatus { get; set; } = null!;
    }

    public class BlockedMemberDto
    {
        public string MemberName { get; set; } = null!;
        public string UserType { get; set; } = null!;
        public string BlockType { get; set; } = null!;
        public string BlockReason { get; set; } = null!;
        public DateTime BlockedAt { get; set; }
    }

    public class MonthlyFineDto
    {
        public string Month { get; set; } = null!;
        public decimal TotalFine { get; set; }
    }

    public class CurrentIssueDto
    {
        public int IssueId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string AccessionNo { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsOverdue { get; set; }
        public string Status { get; set; } = null!;
    }

    public class IssueHistoryDto
    {
        public int IssueId { get; set; }
        public string BookTitle { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public decimal FineAmount { get; set; }
        public bool IsFinePaid { get; set; }
        public string TransactionStatus { get; set; } = null!;
    }

    // ============================================================
    // QR CODE DTOs
    // ============================================================
    public class QRCodeDataDto
    {
        public int CopyId { get; set; }
        public string AccessionNo { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? ISBN { get; set; }
        public string? ShelfLocation { get; set; }
        public string ScanURL { get; set; } = null!;
    }

    public class BookScanInfoDto
    {
        public int CopyId { get; set; }
        public string AccessionNo { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? Publisher { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? ShelfLocation { get; set; }
        public string CopyCondition { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? IssuedTo { get; set; }
    }

    // ============================================================
    // REPORT DTOs
    // ============================================================
    public class IssueReportDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? UserType { get; set; }
        public string? Status { get; set; }
    }

    public class FineReportDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsPaid { get; set; }
    }
}
