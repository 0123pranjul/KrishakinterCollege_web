using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory
{
    public class InvProductService : IInvProductService
    {
        private readonly IInvProductRepository       _repo;
        private readonly IInvCategoryRepository      _catRepo;
        private readonly IInvStockAdjustmentRepository _adjRepo;
        private readonly IWebHostEnvironment         _env;

        public InvProductService(
            IInvProductRepository repo,
            IInvCategoryRepository catRepo,
            IInvStockAdjustmentRepository adjRepo,
            IWebHostEnvironment env)
        {
            _repo    = repo;
            _catRepo = catRepo;
            _adjRepo = adjRepo;
            _env     = env;
        }

        public Task<(List<InvProductListItemDto> Items, int Total)> SearchAsync(InvProductSearchDto filter) =>
            _repo.SearchAsync(filter);

        public Task<InvProduct?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public Task<List<InvProductLookupDto>> SearchLookupAsync(string query) =>
            _repo.SearchLookupAsync(query);

        public async Task<(bool Success, string Message, int ProductId)> CreateAsync(InvProductDto dto, int createdBy)
        {
            var category = await _catRepo.GetByIdAsync(dto.CategoryId);
            if (category == null) return (false, "Category not found.", 0);

            // Generate category code from first 3-4 chars
            var catCode = category.CategoryName.Length >= 3
                ? category.CategoryName.Substring(0, Math.Min(4, category.CategoryName.Length))
                    .ToUpper().Replace(" ", "")
                : category.CategoryName.ToUpper();

            var productCode = await _repo.GenerateProductCodeAsync(dto.CategoryId, catCode);

            // Handle image upload
            string? imagePath = null;
            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "productimages");
                Directory.CreateDirectory(dir);
                var ext      = Path.GetExtension(dto.ProductImage.FileName);
                var fileName = $"{productCode}{ext}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await dto.ProductImage.CopyToAsync(stream);
                imagePath = $"/productimages/{fileName}";
            }

            var product = new InvProduct
            {
                ProductCode      = productCode,
                ProductName      = dto.ProductName.Trim(),
                CategoryId       = dto.CategoryId,
                UnitId           = dto.UnitId,
                CostPrice        = dto.CostPrice,
                SellingPrice     = dto.SellingPrice,
                CurrentStock     = dto.OpeningStock,
                ReorderLevel     = dto.ReorderLevel,
                MaxStockLevel    = dto.MaxStockLevel,
                Description      = dto.Description?.Trim(),
                Hsncode          = dto.HSNCode?.Trim(),
                Gstpercent       = dto.GSTPercent,
                ProductImagePath = imagePath,
                Barcode          = dto.Barcode?.Trim(),
                IsActive         = true,
                CreatedAt        = DateTime.Now,
                CreatedBy        = createdBy
            };

            await _repo.CreateAsync(product);

            // Opening stock adjustment
            if (dto.OpeningStock > 0)
            {
                await _adjRepo.CreateAsync(new InvStockAdjustment
                {
                    ProductId      = product.ProductId,
                    AdjustmentType = "OpeningStock",
                    QuantityBefore = 0,
                    AdjustedQty    = dto.OpeningStock,
                    Reason         = "Opening stock entry",
                    AdjustedBy     = createdBy,
                    AdjustedAt     = DateTime.Now
                });
            }

            return (true, $"Product '{product.ProductName}' created. Code: {productCode}", product.ProductId);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(InvProductDto dto, int updatedBy)
        {
            var product = await _repo.GetByIdAsync(dto.ProductId);
            if (product == null) return (false, "Product not found.");

            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "productimages");
                Directory.CreateDirectory(dir);
                var ext      = Path.GetExtension(dto.ProductImage.FileName);
                var fileName = $"{product.ProductCode}{ext}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await dto.ProductImage.CopyToAsync(stream);
                product.ProductImagePath = $"/productimages/{fileName}";
            }

            product.ProductName   = dto.ProductName.Trim();
            product.CategoryId    = dto.CategoryId;
            product.UnitId        = dto.UnitId;
            product.CostPrice     = dto.CostPrice;
            product.SellingPrice  = dto.SellingPrice;
            product.ReorderLevel  = dto.ReorderLevel;
            product.MaxStockLevel = dto.MaxStockLevel;
            product.Description   = dto.Description?.Trim();
            product.Hsncode       = dto.HSNCode?.Trim();
            product.Gstpercent    = dto.GSTPercent;
            product.Barcode       = dto.Barcode?.Trim();
            product.IsActive      = dto.IsActive;
            product.UpdatedAt     = DateTime.Now;
            product.UpdatedBy     = updatedBy;

            await _repo.UpdateAsync(product);
            return (true, "Product updated.");
        }

        public Task<List<InvLowStockAlertDto>> GetLowStockAsync() => _repo.GetLowStockAsync();
        public Task<List<InvLowStockAlertDto>> GetOutOfStockAsync() => _repo.GetOutOfStockAsync();
    }
}
