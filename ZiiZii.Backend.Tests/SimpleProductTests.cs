// ZiiZii.Backend.Tests/SimpleProductTests.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using ZiiZii.Backend.Core.Entities;
using ZiiZii.Backend.Infrastructure.Data;
using ZiiZii.Backend.Infrastructure.Services;

namespace ZiiZii.Backend.Tests
{
    public class SimpleProductTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateProductAsync_ShouldAddProductToDatabase_Simple()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var product = new Product { Name = "Simple Product", Description = "A simple product", SKU = "SP01", Price = 9.99m };

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProductService(context);
                await service.CreateProductAsync(product);
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Name == "Simple Product");
                Assert.NotNull(savedProduct);
                Assert.Equal("A simple product", savedProduct.Description);
            }
        }
    }
}