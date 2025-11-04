using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZiiZii.Backend.Core.Entities;

namespace ZiiZii.Backend.Core.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryOperationResult> AdjustStockAsync(int variantId, int quantity, string reason, string note = null);
        Task<InventoryOperationResult> ReserveStockAsync(int variantId, int quantity);
        Task<InventoryOperationResult> ReleaseStockAsync(int variantId, int quantity);
        Task<List<LowStockAlert>> GetLowStockAlertsAsync();
        Task<StockSummary> GetStockSummaryAsync();
        Task<List<InventoryLog>> GetInventoryHistoryAsync(int variantId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
