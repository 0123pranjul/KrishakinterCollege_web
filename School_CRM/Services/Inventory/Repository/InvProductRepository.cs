using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory.Repository
{
    public class InvProductRepository : IInvProductRepository
    {
        private readonly LibmanagementContext _db;
        public InvProductRepository(LibmanagementContext db) => _db = db;

        public async Task<(List<InvProductListItemDto> Items, int Total)> SearchAsync(InvProductSearchDto filter)
        {
            var q = _db.InvProducts
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .Where(x => x.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var s = filter.SearchText.Trim().ToLower();
                q = q.Where(x => x.ProductName.ToLower().Contains(s)
                               || x.ProductCode.ToLower().Contains(s)
                               || (x.Barcode != null && x.Barcode.Contains(s)));
            }

            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
                q = q.Where(x => x.CategoryId == filter.CategoryId);

            q = filter.StockStatus switch
            {
                "OutOfStock" => q.Where(x => x.CurrentStock == 0),
                "LowStock"   => q.Where(x => x.CurrentStock > 0 && x.CurrentStock <= x.ReorderLevel),
                "Normal"     => q.Where(x => x.CurrentStock > x.ReorderLevel),
                _            => q
            };

            var total = await q.CountAsync();
            var items = await q
                .OrderBy(x => x.ProductName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new InvProductListItemDto
                {
                    ProductId    = x.ProductId,
                    ProductCode  = x.ProductCode,
                    ProductName  = x.ProductName,
                    CategoryName = x.Category.CategoryName,
                    UnitShort    = x.Unit.UnitShort,
                    CostPrice    = x.CostPrice,
                    SellingPrice = x.SellingPrice,
                    CurrentStock = x.CurrentStock,
                    ReorderLevel = x.ReorderLevel,
                    StockStatus  = x.CurrentStock == 0 ? "OutOfStock"
                                 : x.CurrentStock <= x.ReorderLevel ? "LowStock"
                                 : "InStock",
                    IsActive     = x.IsActive
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<InvProduct?> GetByIdAsync(int id) =>
            await _db.InvProducts
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.ProductId == id);

        public async Task<List<InvProductLookupDto>> SearchLookupAsync(string query)
        {
            var q = query.Trim().ToLower();
            return await _db.InvProducts
                .Include(x => x.Unit)
                .Where(x => x.IsActive && x.CurrentStock > 0 &&
                           (x.ProductName.ToLower().Contains(q) ||
                            x.ProductCode.ToLower().Contains(q) ||
                            (x.Barcode != null && x.Barcode.Contains(q))))
                .Take(10)
                .Select(x => new InvProductLookupDto
                {
                    ProductId    = x.ProductId,
                    ProductCode  = x.ProductCode,
                    ProductName  = x.ProductName,
                    CostPrice    = x.CostPrice,
                    SellingPrice = x.SellingPrice,
                    CurrentStock = x.CurrentStock,
                    UnitShort    = x.Unit.UnitShort,
                    GSTPercent   = x.Gstpercent
                })
                .ToListAsync();
        }

        public async Task<InvProduct> CreateAsync(InvProduct entity)
        {
            _db.InvProducts.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<InvProduct> UpdateAsync(InvProduct entity)
        {
            _db.InvProducts.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateStockAsync(int productId, int delta)
        {
            var product = await _db.InvProducts.FindAsync(productId);
            if (product == null) return false;
            product.CurrentStock = Math.Max(0, product.CurrentStock + delta);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateProductCodeAsync(int categoryId, string catCode)
        {
            var count = await _db.InvProducts.CountAsync(x => x.CategoryId == categoryId);
            return $"INV-{catCode.ToUpper()}-{(count + 1):D3}";
        }

        public async Task<List<InvLowStockAlertDto>> GetLowStockAsync() =>
            await _db.InvProducts
                .Include(x => x.Category)
                .Where(x => x.IsActive && x.CurrentStock > 0 && x.CurrentStock <= x.ReorderLevel)
                .OrderBy(x => x.CurrentStock)
                .Select(x => new InvLowStockAlertDto
                {
                    ProductId    = x.ProductId,
                    ProductCode  = x.ProductCode,
                    ProductName  = x.ProductName,
                    CategoryName = x.Category.CategoryName,
                    CurrentStock = x.CurrentStock,
                    ReorderLevel = x.ReorderLevel
                })
                .ToListAsync();

        public async Task<List<InvLowStockAlertDto>> GetOutOfStockAsync() =>
            await _db.InvProducts
                .Include(x => x.Category)
                .Where(x => x.IsActive && x.CurrentStock == 0)
                .Select(x => new InvLowStockAlertDto
                {
                    ProductId    = x.ProductId,
                    ProductCode  = x.ProductCode,
                    ProductName  = x.ProductName,
                    CategoryName = x.Category.CategoryName,
                    CurrentStock = 0,
                    ReorderLevel = x.ReorderLevel
                })
                .ToListAsync();
    }
}
