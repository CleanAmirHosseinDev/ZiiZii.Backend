using System;

namespace ZiiZii.Backend.Core.Entities
{
    public class StockSummary
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int TotalStock { get; set; }       // مجموع موجودی کل
        public int TotalVariants { get; set; }    // ✅ تعداد کل واریانت‌ها
        public DateTime LastUpdated { get; set; } // آخرین زمان بروزرسانی
    }
}
