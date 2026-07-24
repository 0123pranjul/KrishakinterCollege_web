using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class FinePolicyService : IFinePolicyService
    {
        private readonly IFinePolicyRepository _repo;
        private readonly ILibSettingsRepository _settingsRepo;

        public FinePolicyService(IFinePolicyRepository repo, ILibSettingsRepository settingsRepo)
        {
            _repo         = repo;
            _settingsRepo = settingsRepo;
        }

        public async Task<LibFinePolicy?> GetActivePolicyAsync()
        {
            return await _repo.GetActivePolicyAsync();
        }

        public async Task<List<LibFinePolicy>> GetAllPoliciesAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<(bool Success, string Message)> CreatePolicyAsync(FinePolicyDto dto, int createdBy)
        {
            // Deactivate all existing policies
            await _repo.DeactivateAllAsync();

            var policy = new LibFinePolicy
            {
                PolicyName          = dto.PolicyName.Trim(),
                PerDayFine          = dto.PerDayFine,
                GracePeriodDays     = dto.GracePeriodDays,
                MaxOverdueFine      = dto.MaxOverdueFine,
                MaxBooksForStudent  = dto.MaxBooksForStudent,
                MaxBooksForTeacher  = dto.MaxBooksForTeacher,
                IssueDaysForStudent = dto.IssueDaysForStudent,
                IssueDaysForTeacher = dto.IssueDaysForTeacher,
                DamageFineType      = dto.DamageFineType,
                DamageFineValue     = dto.DamageFineValue,
                LostFineType        = dto.LostFineType,
                LostFineValue       = dto.LostFineValue,
                IsActive            = true,
                CreatedAt           = DateTime.Now,
                CreatedBy           = createdBy
            };

            await _repo.CreateAsync(policy);

            // Update settings
            await _settingsRepo.SetValueAsync("CurrentPolicyId", policy.PolicyId.ToString(), createdBy);

            return (true, "Fine policy created and activated successfully.");
        }

        public decimal CalculateOverdueFine(LibFinePolicy policy, DateOnly dueDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int overdueDays = Math.Max(0,
                (today.ToDateTime(TimeOnly.MinValue) - dueDate.ToDateTime(TimeOnly.MinValue)).Days
                - policy.GracePeriodDays);

            decimal fine = overdueDays * policy.PerDayFine;

            if (policy.MaxOverdueFine.HasValue)
                fine = Math.Min(fine, policy.MaxOverdueFine.Value);

            return fine;
        }

        public decimal CalculateDamageFine(LibFinePolicy policy, decimal copyPrice)
        {
            return policy.DamageFineType == "Percentage"
                ? copyPrice * policy.DamageFineValue / 100
                : policy.DamageFineValue;
        }

        public decimal CalculateLostFine(LibFinePolicy policy, decimal copyPrice)
        {
            return policy.LostFineType switch
            {
                "BookPrice"  => copyPrice,
                "Fixed"      => policy.LostFineValue,
                "Multiplier" => copyPrice * policy.LostFineValue,
                _            => copyPrice
            };
        }
    }
}
