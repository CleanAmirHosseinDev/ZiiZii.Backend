// ZiiZii.Backend.Infrastructure/Services/InventoryService.cs
using Microsoft.EntityFrameworkCore;
using ZiiZiiKids.Core.Entities;
using ZiiZiiKids.Core.Interfaces;
using ZiiZiiKids.Infrastructure.Data;

namespace ZiiZiiKids.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InventoryOperationResult> AdjustStockAsync(int variantId, int quantity, string reason, string note = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == variantId);

                if (variant == null)
                {
                    return new InventoryOperationResult
                    {
                        Success = false,
                        Message = "Product variant not found"
                    };
                }

                var previousStock = variant.StockQuantity;
                variant.StockQuantity += quantity;
                
                // جلوگیری از موجودی منفی
                if (variant.StockQuantity < 0)
                {
                    variant.StockQuantity = 0;
                }

                variant.UpdatedAt = DateTime.UtcNow;

                // ثبت در تاریخچه موجودی
                var inventoryLog = new InventoryLog
                {
                    ProductVariantId = variantId,
                    ChangeType = quantity >= 0 ? "INCREMENT" : "DECREMENT",
                    Quantity = Math.Abs(quantity),
                    PreviousStock = previousStock,
                    NewStock = variant.StockQuantity,
                    Reason = reason,
                    Note = note,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryLogs.Add(inventoryLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // چک کردن هشدار موجودی کم
                await CheckLowStockAlertAsync(variant);

                _logger.LogInformation("Stock adjusted for variant {VariantId}. Change: {Quantity}, New stock: {NewStock}", 
                    variantId, quantity, variant.StockQuantity);

                return new InventoryOperationResult
                {
                    Success = true,
                    Message = "Stock adjusted successfully",
                    NewStock = variant.StockQuantity
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to adjust stock for variant {VariantId}", variantId);
                
                return new InventoryOperationResult
                {
                    Success = false,
                    Message = $"Failed to adjust stock: {ex.Message}"
                };
            }
        }

        public async Task<List<LowStockAlert>> GetLowStockAlertsAsync()
        {
            const int lowStockThreshold = 5;
            
            var lowStockItems = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.StockQuantity <= lowStockThreshold && v.StockQuantity > 0)
                .Select(v => new LowStockAlert
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    Size = v.Size,
                    Color = v.Color,
                    CurrentStock = v.StockQuantity,
                    Threshold = v.LowStockThreshold,
                    LastUpdated = v.UpdatedAt
                })
                .ToListAsync();

            var outOfStockItems = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.StockQuantity == 0)
                .Select(v => new LowStockAlert
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    Size = v.Size,
                    Color = v.Color,
                    CurrentStock = 0,
                    Threshold = v.LowStockThreshold,
                    IsOutOfStock = true,
                    LastUpdated = v.UpdatedAt
                })
                .ToListAsync();

            return lowStockItems.Concat(outOfStockItems).ToList();
        }

        public async Task<StockSummary> GetStockSummaryAsync()
        {
            var variants = await _context.ProductVariants.ToListAsync();
            
            return new StockSummary
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalVariants = variants.Count,
                LowStockItems = variants.Count(v => v.StockQuantity > 0 && v.StockQuantity <= v.LowStockThreshold),
                OutOfStockItems = variants.Count(v => v.StockQuantity == 0),
                TotalInventoryValue = variants.Sum(v => v.StockQuantity * v.Price)
            };
        }

        private async Task CheckLowStockAlertAsync(ProductVariant variant)
        {
            if (variant.StockQuantity <= variant.LowStockThreshold)
            {
                // اینجا می‌تونید:
                // 1. ایمیل به ادمین بفرستید
                // 2. نوتیفیکیشن در سیستم ایجاد کنید
                // 3. به سرویس خارجی اطلاع دهید
                
                _logger.LogWarning("Low stock alert for variant {VariantId}. Current stock: {Stock}, Threshold: {Threshold}", 
                    variant.Id, variant.StockQuantity, variant.LowStockThreshold);
                
                // مثال: ارسال ایمیل
                await SendLowStockNotificationAsync(variant);
            }
        }

        private async Task SendLowStockNotificationAsync(ProductVariant variant)
        {
            // پیاده‌سازی ارسال ایمیل یا نوتیفیکیشن
            // می‌تونید از SendGrid, MailKit, یا سرویس‌های دیگر استفاده کنید
        }
    }

    public class InventoryOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NewStock { get; set; }
    }

    public class LowStockAlert
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public bool IsOutOfStock { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class StockSummary
    {
        public int TotalProducts { get; set; }
        public int TotalVariants { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}