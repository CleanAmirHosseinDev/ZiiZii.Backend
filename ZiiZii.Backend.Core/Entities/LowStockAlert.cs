namespace ZiiZii.Backend.Core.Entities
{
    public class LowStockAlert
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
    }
}