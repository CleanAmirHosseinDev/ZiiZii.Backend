// Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZiiZii.Backend.Core.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string SKU { get; set; }
        
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public ICollection<ProductImage> Images { get; set; }
        public ICollection<ProductVariant> Variants { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
        
        public Product Product { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; }
        
        public Product Product { get; set; }
    }
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        
        [StringLength(50)]
        public string Size { get; set; }
        
        [StringLength(50)]
        public string Color { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        
        public Product Product { get; set; }
    }
}