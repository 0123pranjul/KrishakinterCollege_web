using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    public interface IAssetCategoryRepository
    {
        Task<List<AsmCategory>> GetAllAsync(bool activeOnly = true);
        Task<AsmCategory?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string name, int excludeId = 0);
        Task<AsmCategory> CreateAsync(AsmCategory entity);
        Task<AsmCategory> UpdateAsync(AsmCategory entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IAssetSubCategoryRepository
    {
        Task<List<AsmSubCategory>> GetAllAsync(bool activeOnly = true);
        Task<List<AsmSubCategory>> GetByCategoryAsync(int categoryId);
        Task<AsmSubCategory?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int categoryId, string name, int excludeId = 0);
        Task<AsmSubCategory> CreateAsync(AsmSubCategory entity);
        Task<AsmSubCategory> UpdateAsync(AsmSubCategory entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IAssetLocationRepository
    {
        Task<List<AsmLocation>> GetAllAsync(bool activeOnly = true);
        Task<AsmLocation?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string name, int excludeId = 0);
        Task<AsmLocation> CreateAsync(AsmLocation entity);
        Task<AsmLocation> UpdateAsync(AsmLocation entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IAssetVendorRepository
    {
        Task<List<AsmVendor>> GetAllAsync(bool activeOnly = true);
        Task<AsmVendor?> GetByIdAsync(int id);
        Task<AsmVendor> CreateAsync(AsmVendor entity);
        Task<AsmVendor> UpdateAsync(AsmVendor entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IAssetMasterRepository
    {
        Task<(List<AssetListItemDto> Items, int TotalCount)> SearchAsync(AssetSearchDto filter);
        Task<AsmAsset?> GetByIdAsync(int id);
        Task<AsmAsset?> GetByIdWithUnitsAsync(int id);
        Task<AsmAsset> CreateAsync(AsmAsset entity);
        Task<AsmAsset> UpdateAsync(AsmAsset entity);
        Task<bool> DeactivateAsync(int id, int updatedBy);
        Task<string> GenerateAssetCodeAsync(string categoryCode, int year);
        Task<bool> UpdateUnitCountsAsync(int assetId, int totalDelta, int availableDelta);
    }

    public interface IAssetUnitRepository
    {
        Task<List<AsmAssetUnit>> GetByAssetIdAsync(int assetId);
        Task<AsmAssetUnit?> GetByIdAsync(int unitId);
        Task<AsmAssetUnit?> GetByTagAsync(string assetTag);
        Task<string> GenerateAssetTagAsync(int year);
        Task<AsmAssetUnit> CreateAsync(AsmAssetUnit entity);
        Task<AsmAssetUnit> UpdateAsync(AsmAssetUnit entity);
        Task<bool> UpdateAvailabilityAsync(int unitId, bool isAvailable, string? condition = null,
            string? assignedToType = null, int? assignedToId = null, int? locationId = null);
    }

    public interface IAssetIssueRepository
    {
        Task<AsmIssueTransaction?> GetByIdAsync(int issueId);
        Task<AsmIssueTransaction?> GetOpenIssueByUnitIdAsync(int unitId);
        Task<bool> HasOpenIssueAsync(int unitId);
        Task<List<AsmIssueTransaction>> GetByPersonAsync(string type, int id, bool openOnly = false);
        Task<List<OverdueAssetDto>> GetOverdueListAsync();
        Task<List<RecentIssueDto>> GetRecentIssuedAsync(int count = 10);
        Task<int> GetIssuedCountAsync();
        Task<int> GetOverdueCountAsync();
        Task<AsmIssueTransaction> CreateAsync(AsmIssueTransaction entity);
        Task<AsmIssueTransaction> UpdateAsync(AsmIssueTransaction entity);
    }

    public interface IAssetLocationHistoryRepository
    {
        Task<List<AsmLocationHistory>> GetByUnitIdAsync(int unitId);
        Task<AsmLocationHistory> CreateAsync(AsmLocationHistory entity);
    }

    public interface IAssetMaintenanceRepository
    {
        Task<List<AsmMaintenanceLog>> GetByUnitIdAsync(int unitId);
        Task<AsmMaintenanceLog?> GetByIdAsync(int id);
        Task<List<MaintenanceAlertDto>> GetPendingAsync();
        Task<int> GetPendingCountAsync();
        Task<AsmMaintenanceLog> CreateAsync(AsmMaintenanceLog entity);
        Task<AsmMaintenanceLog> UpdateAsync(AsmMaintenanceLog entity);
    }

    public interface IAssetDamageReportRepository
    {
        Task<List<AsmDamageLossReport>> GetByUnitIdAsync(int unitId);
        Task<AsmDamageLossReport?> GetByIdAsync(int id);
        Task<int> GetOpenCountAsync();
        Task<AsmDamageLossReport> CreateAsync(AsmDamageLossReport entity);
        Task<AsmDamageLossReport> UpdateAsync(AsmDamageLossReport entity);
    }

    public interface IAssetDisposalRepository
    {
        Task<List<AsmDisposalLog>> GetByUnitIdAsync(int unitId);
        Task<AsmDisposalLog?> GetByIdAsync(int id);
        Task<AsmDisposalLog> CreateAsync(AsmDisposalLog entity);
    }
}
