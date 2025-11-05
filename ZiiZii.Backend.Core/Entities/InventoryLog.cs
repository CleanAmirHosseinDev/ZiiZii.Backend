using System;

namespace ZiiZii.Backend.Core.Entities
{
    public class InventoryLog
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public int QuantityChanged { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}