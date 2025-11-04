using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Core.Entities;
using ZiiZii.Backend.Core.Interfaces;
using ZiiZii.Backend.Infrastructure.Data;

namespace ZiiZii.Backend.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryOperationResult> AdjustStockAsync(int variantId, int quantity, string reason, string note = null)
        {
            var variant = await _context.Set<ProductVariant>().FindAsync(variantId);
            if (variant == null)
            {
                return new InventoryOperationResult { Success = false, Message = "Variant not found." };
            }

            variant.StockQuantity += quantity;

            // ذخیره در لاگ
            var log = new InventoryLog
            {
                VariantId = variantId,
                QuantityChanged = quantity,
                Reason = reason,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<InventoryLog>().Add(log);
            await _context.SaveChangesAsync();

            return new InventoryOperationResult { Success = true, Message = "Stock adjusted successfully." };
        }

        public async Task<InventoryOperationResult> ReserveStockAsync(int variantId, int quantity)
        {
            var variant = await _context.Set<ProductVariant>().FindAsync(variantId);
            if (variant == null || variant.StockQuantity < quantity)
            {
                return new InventoryOperationResult { Success = false, Message = "Not enough stock available." };
            }

            variant.StockQuantity -= quantity;
            await _context.SaveChangesAsync();

            return new InventoryOperationResult { Success = true, Message = "Stock reserved successfully." };
        }

        public async Task<InventoryOperationResult> ReleaseStockAsync(int variantId, int quantity)
        {
            var variant = await _context.Set<ProductVariant>().FindAsync(variantId);
            if (variant == null)
            {
                return new InventoryOperationResult { Success = false, Message = "Variant not found." };
            }

            variant.StockQuantity += quantity;
            await _context.SaveChangesAsync();

            return new InventoryOperationResult { Success = true, Message = "Stock released successfully." };
        }

        public async Task<List<LowStockAlert>> GetLowStockAlertsAsync()
        {
            var lowStockVariants = await _context.Set<ProductVariant>()
                .Where(v => v.StockQuantity < 5)
                .Select(v => new LowStockAlert
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    CurrentStock = v.StockQuantity
                })
                .ToListAsync();

            return lowStockVariants;
        }

        public async Task<StockSummary> GetStockSummaryAsync()
        {
            var totalStock = await _context.Set<ProductVariant>().SumAsync(v => v.StockQuantity);
            var totalVariants = await _context.Set<ProductVariant>().CountAsync();

            return new StockSummary
            {
                TotalStock = totalStock,
                TotalVariants = totalVariants,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<List<InventoryLog>> GetInventoryHistoryAsync(int variantId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Set<InventoryLog>().Where(l => l.VariantId == variantId);

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(l => l.CreatedAt <= toDate.Value);

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}
