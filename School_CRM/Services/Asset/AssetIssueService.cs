using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset
{
    public class AssetIssueService : IAssetIssueService
    {
        private readonly IAssetUnitRepository         _unitRepo;
        private readonly IAssetIssueRepository        _issueRepo;
        private readonly IAssetMasterRepository       _assetRepo;
        private readonly IAssetLocationHistoryRepository _histRepo;
        private readonly IAssetDamageReportRepository _damageRepo;
        private readonly LibmanagementContext          _db;

        public AssetIssueService(
            IAssetUnitRepository unitRepo,
            IAssetIssueRepository issueRepo,
            IAssetMasterRepository assetRepo,
            IAssetLocationHistoryRepository histRepo,
            IAssetDamageReportRepository damageRepo,
            LibmanagementContext db)
        {
            _unitRepo   = unitRepo;
            _issueRepo  = issueRepo;
            _assetRepo  = assetRepo;
            _histRepo   = histRepo;
            _damageRepo = damageRepo;
            _db         = db;
        }

        public async Task<AssetEligibilityDto> CheckAssetEligibilityAsync(string assetTag)
        {
            var unit = await _unitRepo.GetByTagAsync(assetTag);
            if (unit == null)
                return new AssetEligibilityDto { IsEligible = false, Message = "Asset tag not found." };

            if (!unit.IsAvailable)
                return new AssetEligibilityDto { IsEligible = false, IsAvailable = false,
                    UnitCondition = unit.UnitCondition, Message = "Asset is already issued or unavailable." };

            if (unit.UnitCondition is "UnderRepair" or "Disposed" or "Lost")
                return new AssetEligibilityDto { IsEligible = false, IsAvailable = false,
                    UnitCondition = unit.UnitCondition, Message = $"Cannot issue — condition: {unit.UnitCondition}." };

            if (!unit.Asset.IsIssuable)
                return new AssetEligibilityDto { IsEligible = false, IsAvailable = true,
                    UnitCondition = unit.UnitCondition, Message = "This asset type is not issuable." };

            return new AssetEligibilityDto
            {
                IsEligible    = true,
                IsAvailable   = true,
                IsIssuable    = true,
                UnitCondition = unit.UnitCondition,
                UnitDetails   = new AssetUnitDto
                {
                    UnitId        = unit.UnitId,
                    AssetId       = unit.AssetId,
                    AssetTag      = unit.AssetTag,
                    AssetName     = unit.Asset.AssetName,
                    CategoryName  = unit.Asset.Category.CategoryName,
                    LocationName  = unit.CurrentLocation?.LocationName,
                    UnitCondition = unit.UnitCondition
                }
            };
        }

        public async Task<(bool Success, string Message, int IssueId)> IssueAsync(IssueAssetDto dto)
        {
            var eligibility = await CheckAssetEligibilityAsync(dto.AssetTag);
            if (!eligibility.IsEligible)
                return (false, eligibility.Message ?? "Asset not eligible.", 0);

            var unit = await _unitRepo.GetByTagAsync(dto.AssetTag);
            if (unit == null) return (false, "Unit not found.", 0);

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var issue = new AsmIssueTransaction
                {
                    UnitId             = unit.UnitId,
                    IssuedToType       = dto.IssuedToType,
                    IssuedToId         = dto.IssuedToId,
                    IssuedBy           = dto.IssuedBy,
                    IssueDate          = dto.IssueDate,
                    ExpectedReturnDate = dto.ExpectedReturnDate,
                    Purpose            = dto.Purpose,
                    IsReturned         = false,
                    ConditionOnIssue   = unit.UnitCondition,
                    IsDamaged          = false,
                    IsLost             = false,
                    DamageFine         = 0,
                    IsFinePaid         = false,
                    TransactionStatus  = dto.TransactionStatus,
                    Remarks            = dto.Remarks,
                    CreatedAt          = DateTime.Now
                };

                await _issueRepo.CreateAsync(issue);

                // Update unit
                int? newLocationId = dto.IssuedToType == "Location" ? dto.IssuedToId : unit.CurrentLocationId;
                await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, false, null,
                    dto.IssuedToType, dto.IssuedToId, newLocationId);

                // Update asset available count
                await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, 0, -1);

                // Log location change if issued to a location
                if (dto.IssuedToType == "Location" && unit.CurrentLocationId != dto.IssuedToId)
                {
                    await _histRepo.CreateAsync(new AsmLocationHistory
                    {
                        UnitId         = unit.UnitId,
                        FromLocationId = unit.CurrentLocationId,
                        ToLocationId   = dto.IssuedToId,
                        MovedBy        = dto.IssuedBy,
                        MoveDate       = DateTime.Now,
                        Reason         = $"Issued to location"
                    });
                }

                await tx.CommitAsync();
                return (true, "Asset issued successfully.", issue.IssueId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Issue failed: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)> ReturnAsync(ReturnAssetDto dto)
        {
            var issue = await _issueRepo.GetOpenIssueByUnitIdAsync(dto.UnitId);
            if (issue == null) return (false, "No open issue found for this asset.");

            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null) return (false, "Unit not found.");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var today  = DateOnly.FromDateTime(DateTime.Today);
                var status = dto.ConditionOnReturn switch
                {
                    "Damaged" => "Damaged",
                    "Lost"    => "Lost",
                    _         => "Returned"
                };

                issue.ReturnDate        = today;
                issue.IsReturned        = true;
                issue.ReturnedTo        = dto.ReturnedTo;
                issue.ConditionOnReturn = dto.ConditionOnReturn;
                issue.IsDamaged         = dto.IsDamaged;
                issue.IsLost            = dto.IsLost;
                issue.DamageFine        = dto.DamageFine;
                issue.TransactionStatus = status;
                issue.Remarks           = dto.Remarks;

                await _issueRepo.UpdateAsync(issue);

                bool makeAvailable = dto.ConditionOnReturn != "Lost";

                await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, makeAvailable,
                    dto.ConditionOnReturn, null, null, null);

                if (makeAvailable)
                    await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, 0, 1);

                // Create damage/loss report if needed
                if (dto.IsDamaged || dto.IsLost)
                {
                    await _damageRepo.CreateAsync(new AsmDamageLossReport
                    {
                        UnitId          = unit.UnitId,
                        IssueId         = issue.IssueId,
                        ReportType      = dto.IsLost ? "Loss" : "Damage",
                        ReportedBy      = dto.ReturnedTo,
                        ReportDate      = today,
                        Description     = dto.Remarks ?? $"Reported on return. Condition: {dto.ConditionOnReturn}",
                        ResponsibleType = issue.IssuedToType,
                        ResponsibleId   = issue.IssuedToId,
                        EstimatedLoss   = dto.DamageFine,
                        FineImposed     = dto.DamageFine,
                        Status          = "Open",
                        CreatedAt       = DateTime.Now
                    });

                    if (dto.IsLost)
                        await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, -1, 0);
                }

                await tx.CommitAsync();
                return (true, dto.DamageFine > 0
                    ? $"Asset returned. Fine of ₹{dto.DamageFine:F2} applied."
                    : "Asset returned successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Return failed: {ex.Message}");
            }
        }

        public async Task<ReturnAssetDto?> GetReturnInfoAsync(string assetTag)
        {
            var unit = await _unitRepo.GetByTagAsync(assetTag);
            if (unit == null) return null;

            var issue = await _issueRepo.GetOpenIssueByUnitIdAsync(unit.UnitId);
            if (issue == null) return null;

            var today = DateOnly.FromDateTime(DateTime.Today);
            int days  = (today.ToDateTime(TimeOnly.MinValue) - issue.IssueDate.ToDateTime(TimeOnly.MinValue)).Days;

            return new ReturnAssetDto
            {
                AssetTag           = assetTag,
                IssueId            = issue.IssueId,
                UnitId             = unit.UnitId,
                AssetName          = unit.Asset.AssetName,
                IssueDate          = issue.IssueDate,
                ExpectedReturnDate = issue.ExpectedReturnDate,
                DaysWithMember     = days,
                ConditionOnReturn  = "Good"
            };
        }

        public async Task<List<OverdueAssetDto>> GetOverdueAsync() =>
            await _issueRepo.GetOverdueListAsync();

        public async Task<(bool Success, string Message)> MoveAssetAsync(MoveAssetDto dto)
        {
            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null) return (false, "Unit not found.");

            if (await _issueRepo.HasOpenIssueAsync(dto.UnitId))
                return (false, "Asset is currently issued. Return it before moving.");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await _histRepo.CreateAsync(new AsmLocationHistory
                {
                    UnitId         = dto.UnitId,
                    FromLocationId = unit.CurrentLocationId,
                    ToLocationId   = dto.ToLocationId,
                    MovedBy        = dto.MovedBy,
                    MoveDate       = dto.MoveDate,
                    Reason         = dto.Reason
                });

                await _unitRepo.UpdateAvailabilityAsync(dto.UnitId, unit.IsAvailable,
                    null, null, null, dto.ToLocationId);

                await tx.CommitAsync();
                return (true, "Asset moved successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Move failed: {ex.Message}");
            }
        }
    }
}
