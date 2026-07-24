using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class FinePaymentService : IFinePaymentService
    {
        private readonly IFinePaymentRepository _paymentRepo;
        private readonly IIssueTransactionRepository _issueRepo;
        private readonly IMemberBlockRepository _blockRepo;
        private readonly ILibraryMemberService _memberService;

        public FinePaymentService(
            IFinePaymentRepository paymentRepo,
            IIssueTransactionRepository issueRepo,
            IMemberBlockRepository blockRepo,
            ILibraryMemberService memberService)
        {
            _paymentRepo  = paymentRepo;
            _issueRepo    = issueRepo;
            _blockRepo    = blockRepo;
            _memberService = memberService;
        }

        public async Task<FinePaymentDto?> GetFineDetailsAsync(int issueId)
        {
            var issue = await _issueRepo.GetByIdAsync(issueId);
            if (issue == null) return null;

            decimal alreadyPaid = await _paymentRepo.GetTotalPaidAsync(issueId);
            decimal remaining   = issue.FineAmount - alreadyPaid;

            var member = await _memberService.GetMemberAsync(issue.UserType, issue.UserId);

            return new FinePaymentDto
            {
                IssueId         = issueId,
                UserType        = issue.UserType,
                UserId          = issue.UserId,
                MemberName      = member?.Name ?? $"{issue.UserType} #{issue.UserId}",
                BookTitle       = issue.Copy.Book.Title,
                TotalFine       = issue.FineAmount,
                AlreadyPaid     = alreadyPaid,
                RemainingAmount = remaining
            };
        }

        public async Task<(bool Success, string Message, string? ReceiptNo)> CollectFineAsync(FinePaymentDto dto)
        {
            var issue = await _issueRepo.GetByIdAsync(dto.IssueId);
            if (issue == null)
                return (false, "Issue transaction not found.", null);

            decimal alreadyPaid = await _paymentRepo.GetTotalPaidAsync(dto.IssueId);
            decimal remaining   = issue.FineAmount - alreadyPaid;

            if (dto.AmountPaid > remaining)
                return (false, $"Amount paid (₹{dto.AmountPaid:F2}) exceeds remaining fine (₹{remaining:F2}).", null);

            // Generate receipt number
            var receiptNo = dto.ReceiptNo ?? $"RCP-{DateTime.Now:yyyyMMddHHmmss}";

            var payment = new LibFinePayment
            {
                IssueId     = dto.IssueId,
                UserType    = dto.UserType,
                UserId      = dto.UserId,
                AmountPaid  = dto.AmountPaid,
                PaymentMode = dto.PaymentMode,
                PaymentDate = DateTime.Now,
                CollectedBy = dto.CollectedBy,
                ReceiptNo   = receiptNo,
                Remarks     = dto.Remarks
            };

            await _paymentRepo.CreateAsync(payment);

            decimal totalPaid = alreadyPaid + dto.AmountPaid;

            // Check if fully paid
            if (totalPaid >= issue.FineAmount)
            {
                issue.IsFinePaid  = true;
                issue.FinePaidDate = DateTime.Now;
                issue.FinePaidBy  = dto.CollectedBy;
                await _issueRepo.UpdateAsync(issue);

                // Unblock member
                await _blockRepo.UnblockMemberAsync(
                    dto.UserType, dto.UserId, dto.CollectedBy, "Fine paid in full");
            }

            return (true, $"Payment of ₹{dto.AmountPaid:F2} collected. Receipt: {receiptNo}", receiptNo);
        }
    }
}
