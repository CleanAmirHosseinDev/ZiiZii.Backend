using Microsoft.EntityFrameworkCore;
using ZiiZiiKids.Core.Entities;

namespace ZiiZiiKids.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.HasQueryFilter(p => p.IsActive);
            });
        }
    }
}