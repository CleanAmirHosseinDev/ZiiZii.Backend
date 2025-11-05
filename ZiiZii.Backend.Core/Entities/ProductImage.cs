namespace ZiiZii.Backend.Core.Entities
{
	public class ProductImage
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ImageUrl { get; set; } = string.Empty;
		public bool IsMain { get; set; }
		public int SortOrder { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Product Product { get; set; } = null!;
	}
}