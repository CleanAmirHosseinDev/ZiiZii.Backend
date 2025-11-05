using System;

namespace ZiiZii.Backend.Core.Entities
{
    public class StockSummary
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int TotalStock { get; set; }
        public int TotalVariants { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}