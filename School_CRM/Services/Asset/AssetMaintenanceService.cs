using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset
{
    public class AssetMaintenanceService : IAssetMaintenanceService
    {
        private readonly IAssetMaintenanceRepository _repo;
        private readonly IAssetUnitRepository        _unitRepo;
        private readonly IAssetMasterRepository      _assetRepo;

        public AssetMaintenanceService(
            IAssetMaintenanceRepository repo,
            IAssetUnitRepository unitRepo,
            IAssetMasterRepository assetRepo)
        {
            _repo      = repo;
            _unitRepo  = unitRepo;
            _assetRepo = assetRepo;
        }

        public Task<List<AsmMaintenanceLog>> GetByUnitAsync(int unitId) => _repo.GetByUnitIdAsync(unitId);
        public Task<AsmMaintenanceLog?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(MaintenanceLogDto dto)
        {
            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null) return (false, "Asset unit not found.");

            var log = new AsmMaintenanceLog
            {
                UnitId          = dto.UnitId,
                MaintenanceType = dto.MaintenanceType,
                Description     = dto.Description,
                ServicedBy      = dto.ServicedBy,
                VendorId        = dto.VendorId,
                StartDate       = dto.StartDate,
                CompletionDate  = dto.CompletionDate,
                Cost            = dto.Cost,
                Status          = dto.Status,
                ConditionBefore = dto.ConditionBefore ?? unit.UnitCondition,
                ConditionAfter  = dto.ConditionAfter,
                BillNo          = dto.BillNo,
                Remarks         = dto.Remarks,
                CreatedBy       = dto.CreatedBy,
                CreatedAt       = DateTime.Now
            };

            await _repo.CreateAsync(log);

            // Update unit condition based on maintenance status
            if (dto.MaintenanceType == "Repair" && dto.Status == "InProgress")
            {
                await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, false, "UnderRepair");
                await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, 0, -1);
            }
            else if (dto.Status == "Completed" && !string.IsNullOrEmpty(dto.ConditionAfter))
            {
                bool wasUnderRepair = unit.UnitCondition == "UnderRepair";
                await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, true, dto.ConditionAfter);
                if (wasUnderRepair)
                    await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, 0, 1);
            }

            return (true, "Maintenance log created.");
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(
            int id, string status, string? conditionAfter, int updatedBy)
        {
            var log = await _repo.GetByIdAsync(id);
            if (log == null) return (false, "Maintenance log not found.");

            log.Status         = status;
            log.ConditionAfter = conditionAfter ?? log.ConditionAfter;
            if (status == "Completed") log.CompletionDate = DateOnly.FromDateTime(DateTime.Today);

            await _repo.UpdateAsync(log);

            if (status == "Completed" && !string.IsNullOrEmpty(conditionAfter))
            {
                var unit = await _unitRepo.GetByIdAsync(log.UnitId);
                if (unit != null)
                {
                    bool wasUnderRepair = unit.UnitCondition == "UnderRepair";
                    await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, true, conditionAfter);
                    if (wasUnderRepair)
                        await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, 0, 1);
                }
            }

            return (true, "Status updated.");
        }
    }

    public class AssetDamageReportService : IAssetDamageReportService
    {
        private readonly IAssetDamageReportRepository _repo;
        private readonly IAssetUnitRepository         _unitRepo;
        private readonly IAssetMasterRepository       _assetRepo;

        public AssetDamageReportService(
            IAssetDamageReportRepository repo,
            IAssetUnitRepository unitRepo,
            IAssetMasterRepository assetRepo)
        {
            _repo      = repo;
            _unitRepo  = unitRepo;
            _assetRepo = assetRepo;
        }

        public Task<List<AsmDamageLossReport>> GetByUnitAsync(int unitId) => _repo.GetByUnitIdAsync(unitId);
        public Task<AsmDamageLossReport?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(DamageLossReportDto dto)
        {
            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null) return (false, "Asset unit not found.");

            await _repo.CreateAsync(new AsmDamageLossReport
            {
                UnitId          = dto.UnitId,
                IssueId         = dto.IssueId,
                ReportType      = dto.ReportType,
                ReportedBy      = dto.ReportedBy,
                ReportDate      = dto.ReportDate,
                Description     = dto.Description,
                ResponsibleType = dto.ResponsibleType,
                ResponsibleId   = dto.ResponsibleId,
                EstimatedLoss   = dto.EstimatedLoss,
                FineImposed     = dto.FineImposed,
                Status          = "Open",
                Remarks         = dto.Remarks,
                CreatedAt       = DateTime.Now
            });

            if (dto.ReportType == "Loss")
            {
                await _unitRepo.UpdateAvailabilityAsync(unit.UnitId, false, "Lost");
                await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, -1, 0);
            }

            return (true, "Report created.");
        }

        public async Task<(bool Success, string Message)> ResolveAsync(int reportId, string actionTaken, int resolvedBy)
        {
            var report = await _repo.GetByIdAsync(reportId);
            if (report == null) return (false, "Report not found.");

            report.Status      = "Resolved";
            report.ActionTaken = actionTaken;
            report.ResolvedBy  = resolvedBy;
            report.ResolvedDate = DateOnly.FromDateTime(DateTime.Today);
            await _repo.UpdateAsync(report);
            return (true, "Report resolved.");
        }
    }

    public class AssetDisposalService : IAssetDisposalService
    {
        private readonly IAssetDisposalRepository _repo;
        private readonly IAssetUnitRepository     _unitRepo;
        private readonly IAssetMasterRepository   _assetRepo;
        private readonly IAssetIssueRepository    _issueRepo;

        public AssetDisposalService(
            IAssetDisposalRepository repo,
            IAssetUnitRepository unitRepo,
            IAssetMasterRepository assetRepo,
            IAssetIssueRepository issueRepo)
        {
            _repo      = repo;
            _unitRepo  = unitRepo;
            _assetRepo = assetRepo;
            _issueRepo = issueRepo;
        }

        public async Task<(bool Success, string Message)> DisposeAsync(DisposalDto dto)
        {
            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null) return (false, "Asset unit not found.");

            if (await _issueRepo.HasOpenIssueAsync(dto.UnitId))
                return (false, "Asset is currently issued. Return it before disposal.");

            await _repo.CreateAsync(new AsmDisposalLog
            {
                UnitId       = dto.UnitId,
                DisposalType = dto.DisposalType,
                DisposalDate = dto.DisposalDate,
                SaleValue    = dto.SaleValue,
                DisposedTo   = dto.DisposedTo,
                AuthorizedBy = dto.AuthorizedBy,
                Reason       = dto.Reason,
                Remarks      = dto.Remarks,
                CreatedAt    = DateTime.Now,
                CreatedBy    = dto.CreatedBy
            });

            // Mark unit as disposed and inactive
            unit.IsActive      = false;
            unit.UnitCondition = "Disposed";
            unit.IsAvailable   = false;
            await _unitRepo.UpdateAsync(unit);

            // Reduce asset counts
            bool wasAvailable = unit.IsAvailable;
            await _assetRepo.UpdateUnitCountsAsync(unit.AssetId, -1, wasAvailable ? -1 : 0);

            return (true, "Asset disposed successfully.");
        }

        public Task<List<AsmDisposalLog>> GetByUnitAsync(int unitId) => _repo.GetByUnitIdAsync(unitId);
    }
}
