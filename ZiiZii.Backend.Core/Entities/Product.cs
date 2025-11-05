// Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZiiZii.Backend.Core.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
        public Brand Brand { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}