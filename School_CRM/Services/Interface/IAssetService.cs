using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    public interface IAssetCategoryService
    {
        Task<List<AsmCategory>> GetAllAsync(bool activeOnly = true);
        Task<AsmCategory?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(AssetCategoryDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(AssetCategoryDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IAssetSubCategoryService
    {
        Task<List<AsmSubCategory>> GetAllAsync(bool activeOnly = true);
        Task<List<AsmSubCategory>> GetByCategoryAsync(int categoryId);
        Task<AsmSubCategory?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(AssetSubCategoryDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(AssetSubCategoryDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IAssetLocationService
    {
        Task<List<AsmLocation>> GetAllAsync(bool activeOnly = true);
        Task<AsmLocation?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(AssetLocationDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(AssetLocationDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IAssetVendorService
    {
        Task<List<AsmVendor>> GetAllAsync(bool activeOnly = true);
        Task<AsmVendor?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(AssetVendorDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(AssetVendorDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IAssetMasterService
    {
        Task<(List<AssetListItemDto> Items, int TotalCount)> SearchAsync(AssetSearchDto filter);
        Task<AsmAsset?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int AssetId)> CreateAsync(AssetMasterDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(AssetMasterDto dto, int updatedBy);
        Task<(bool Success, string Message)> AddUnitsAsync(AddUnitsDto dto, int createdBy);
        Task<List<AsmAssetUnit>> GetUnitsAsync(int assetId);
        Task<AssetScanInfoDto?> GetScanInfoAsync(string assetTag);
        Task<byte[]?> GetQRImageAsync(string assetTag);
    }

    public interface IAssetIssueService
    {
        Task<AssetEligibilityDto> CheckAssetEligibilityAsync(string assetTag);
        Task<(bool Success, string Message, int IssueId)> IssueAsync(IssueAssetDto dto);
        Task<(bool Success, string Message)> ReturnAsync(ReturnAssetDto dto);
        Task<ReturnAssetDto?> GetReturnInfoAsync(string assetTag);
        Task<List<OverdueAssetDto>> GetOverdueAsync();
        Task<(bool Success, string Message)> MoveAssetAsync(MoveAssetDto dto);
    }

    public interface IAssetMaintenanceService
    {
        Task<List<AsmMaintenanceLog>> GetByUnitAsync(int unitId);
        Task<AsmMaintenanceLog?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(MaintenanceLogDto dto);
        Task<(bool Success, string Message)> UpdateStatusAsync(int id, string status, string? conditionAfter, int updatedBy);
    }

    public interface IAssetDamageReportService
    {
        Task<List<AsmDamageLossReport>> GetByUnitAsync(int unitId);
        Task<AsmDamageLossReport?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(DamageLossReportDto dto);
        Task<(bool Success, string Message)> ResolveAsync(int reportId, string actionTaken, int resolvedBy);
    }

    public interface IAssetDisposalService
    {
        Task<(bool Success, string Message)> DisposeAsync(DisposalDto dto);
        Task<List<AsmDisposalLog>> GetByUnitAsync(int unitId);
    }

    public interface IAssetDashboardService
    {
        Task<AssetAdminDashboardDto> GetAdminDashboardAsync();
        Task<AssetMemberDashboardDto> GetMemberDashboardAsync(string userType, int userId);
    }

    public interface IAssetPersonService
    {
        Task<List<PersonLookupDto>> GetPersonListAsync(string type, string? search = null);
        Task<PersonLookupDto?> GetPersonAsync(string type, int id);
    }
}
