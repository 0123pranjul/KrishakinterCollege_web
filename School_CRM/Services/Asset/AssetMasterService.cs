using System.Text.Json;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset
{
    public class AssetMasterService : IAssetMasterService
    {
        private readonly IAssetMasterRepository _assetRepo;
        private readonly IAssetUnitRepository   _unitRepo;
        private readonly AssetQRCodeService     _qrService;
        private readonly IWebHostEnvironment    _env;

        public AssetMasterService(
            IAssetMasterRepository assetRepo,
            IAssetUnitRepository unitRepo,
            AssetQRCodeService qrService,
            IWebHostEnvironment env)
        {
            _assetRepo = assetRepo;
            _unitRepo  = unitRepo;
            _qrService = qrService;
            _env       = env;
        }

        public async Task<(List<AssetListItemDto> Items, int TotalCount)> SearchAsync(AssetSearchDto filter) =>
            await _assetRepo.SearchAsync(filter);

        public async Task<AsmAsset?> GetByIdAsync(int id) =>
            await _assetRepo.GetByIdAsync(id);

        public async Task<(bool Success, string Message, int AssetId)> CreateAsync(AssetMasterDto dto, int createdBy)
        {
            // Generate asset code using first 4 chars of category name
            var catCode = (dto.CategoryName ?? "MISC").Length >= 4
                ? dto.CategoryName!.Substring(0, 4).ToUpper()
                : (dto.CategoryName ?? "MISC").ToUpper();

            var assetCode = await _assetRepo.GenerateAssetCodeAsync(catCode, DateTime.Today.Year);

            // Handle image upload
            string? imagePath = null;
            if (dto.AssetImage != null && dto.AssetImage.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "assetimages");
                Directory.CreateDirectory(dir);
                var fileName = $"{assetCode}_{Path.GetExtension(dto.AssetImage.FileName)}";
                var filePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.AssetImage.CopyToAsync(stream);
                imagePath = $"/assetimages/{fileName}";
            }

            var asset = new AsmAsset
            {
                AssetName      = dto.AssetName.Trim(),
                AssetCode      = assetCode,
                CategoryId     = dto.CategoryId,
                SubCategoryId  = dto.SubCategoryId,
                Brand          = dto.Brand?.Trim(),
                Model          = dto.Model?.Trim(),
                Specifications = dto.Specifications?.Trim(),
                UnitPrice      = dto.UnitPrice,
                TotalUnits     = dto.NumberOfUnits,
                AvailableUnits = dto.NumberOfUnits,
                IsIssuable     = dto.IsIssuable,
                AssetImagePath = imagePath,
                IsActive       = true,
                CreatedAt      = DateTime.Now,
                CreatedBy      = createdBy
            };

            await _assetRepo.CreateAsync(asset);
            await CreateUnitsAsync(asset, dto.NumberOfUnits, dto, createdBy);

            return (true, $"Asset '{asset.AssetName}' created with {dto.NumberOfUnits} unit(s).", asset.AssetId);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AssetMasterDto dto, int updatedBy)
        {
            var asset = await _assetRepo.GetByIdAsync(dto.AssetId);
            if (asset == null) return (false, "Asset not found.");

            asset.AssetName      = dto.AssetName.Trim();
            asset.CategoryId     = dto.CategoryId;
            asset.SubCategoryId  = dto.SubCategoryId;
            asset.Brand          = dto.Brand?.Trim();
            asset.Model          = dto.Model?.Trim();
            asset.Specifications = dto.Specifications?.Trim();
            asset.UnitPrice      = dto.UnitPrice;
            asset.IsIssuable     = dto.IsIssuable;
            asset.UpdatedAt      = DateTime.Now;
            asset.UpdatedBy      = updatedBy;

            if (dto.AssetImage != null && dto.AssetImage.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "assetimages");
                Directory.CreateDirectory(dir);
                var fileName = $"{asset.AssetCode}_{Path.GetExtension(dto.AssetImage.FileName)}";
                var filePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.AssetImage.CopyToAsync(stream);
                asset.AssetImagePath = $"/assetimages/{fileName}";
            }

            await _assetRepo.UpdateAsync(asset);
            return (true, "Asset updated successfully.");
        }

        public async Task<(bool Success, string Message)> AddUnitsAsync(AddUnitsDto dto, int createdBy)
        {
            var asset = await _assetRepo.GetByIdAsync(dto.AssetId);
            if (asset == null) return (false, "Asset not found.");

            var masterDto = new AssetMasterDto
            {
                PurchaseDate      = dto.PurchaseDate,
                VendorId          = dto.VendorId,
                InvoiceNo         = dto.InvoiceNo,
                WarrantyExpiry    = dto.WarrantyExpiry,
                AMCExpiry         = dto.AMCExpiry,
                DefaultLocationId = dto.DefaultLocationId,
                PurchasePrice     = dto.PurchasePrice,
                CategoryName      = null
            };

            await CreateUnitsAsync(asset, dto.NumberOfUnits, masterDto, createdBy);
            await _assetRepo.UpdateUnitCountsAsync(asset.AssetId, dto.NumberOfUnits, dto.NumberOfUnits);

            return (true, $"{dto.NumberOfUnits} unit(s) added successfully.");
        }

        public async Task<List<AsmAssetUnit>> GetUnitsAsync(int assetId) =>
            await _unitRepo.GetByAssetIdAsync(assetId);

        public async Task<AssetScanInfoDto?> GetScanInfoAsync(string assetTag)
        {
            var unit = await _unitRepo.GetByTagAsync(assetTag);
            if (unit == null) return null;

            return new AssetScanInfoDto
            {
                UnitId          = unit.UnitId,
                AssetTag        = unit.AssetTag,
                AssetName       = unit.Asset.AssetName,
                AssetCode       = unit.Asset.AssetCode,
                CategoryName    = unit.Asset.Category.CategoryName,
                SubCategoryName = unit.Asset.SubCategory?.SubCategoryName,
                Brand           = unit.Asset.Brand,
                Model           = unit.Asset.Model,
                CurrentLocation = unit.CurrentLocation?.LocationName,
                UnitCondition   = unit.UnitCondition,
                IsAvailable     = unit.IsAvailable,
                WarrantyExpiry  = unit.WarrantyExpiry,
                AMCExpiry       = unit.Amcexpiry
            };
        }

        public async Task<byte[]?> GetQRImageAsync(string assetTag) =>
            _qrService.GetQRImage(assetTag);

        // ── Private helpers ──────────────────────────────────────────────
        private async Task CreateUnitsAsync(AsmAsset asset, int count, AssetMasterDto dto, int createdBy)
        {
            var year    = DateTime.Today.Year;
            var baseUrl = "https://yourdomain.com/Assets/Scan/";

            for (int i = 0; i < count; i++)
            {
                var tag = await _unitRepo.GenerateAssetTagAsync(year);

                var qrData = new AssetQRDataDto
                {
                    UnitId          = 0,
                    AssetTag        = tag,
                    AssetName       = asset.AssetName,
                    Category        = asset.Category?.CategoryName ?? "",
                    SubCategory     = asset.SubCategory?.SubCategoryName,
                    Brand           = asset.Brand,
                    Model           = asset.Model,
                    CurrentLocation = null,
                    ScanURL         = $"{baseUrl}{tag}"
                };

                var unit = new AsmAssetUnit
                {
                    AssetId           = asset.AssetId,
                    AssetTag          = tag,
                    QrcodeData        = JsonSerializer.Serialize(qrData),
                    PurchaseDate      = dto.PurchaseDate,
                    PurchasePrice     = dto.PurchasePrice > 0 ? dto.PurchasePrice : asset.UnitPrice,
                    InvoiceNo         = dto.InvoiceNo,
                    VendorId          = dto.VendorId,
                    WarrantyExpiry    = dto.WarrantyExpiry,
                    Amcexpiry         = dto.AMCExpiry,
                    CurrentLocationId = dto.DefaultLocationId,
                    UnitCondition     = "Good",
                    IsAvailable       = true,
                    IsActive          = true,
                    CreatedAt         = DateTime.Now,
                    CreatedBy         = createdBy
                };

                await _unitRepo.CreateAsync(unit);

                // Generate QR image and update path
                qrData.UnitId = unit.UnitId;
                var imagePath = _qrService.GenerateQRCode(qrData);
                unit.QrcodeData      = JsonSerializer.Serialize(qrData);
                unit.QrcodeImagePath = imagePath;
                await _unitRepo.UpdateAsync(unit);
            }
        }
    }
}
