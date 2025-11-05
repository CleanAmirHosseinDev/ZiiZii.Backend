namespace ZiiZii.Backend.Core.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string Body { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}