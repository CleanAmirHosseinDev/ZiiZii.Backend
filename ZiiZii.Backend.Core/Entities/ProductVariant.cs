namespace ZiiZii.Backend.Core.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? SKU { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}