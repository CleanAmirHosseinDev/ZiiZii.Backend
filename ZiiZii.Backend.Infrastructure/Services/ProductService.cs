// ZiiZii.Backend.Infrastructure/Services/ProductService.cs
using Microsoft.EntityFrameworkCore;
using ZiiZiiKids.Core.Entities;
using ZiiZiiKids.Core.Interfaces;
using ZiiZiiKids.Infrastructure.Data;

namespace ZiiZiiKids.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Product>> GetProductsAsync(ProductQueryParams queryParams)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // فیلترها
            if (!string.IsNullOrEmpty(queryParams.Category))
            {
                query = query.Where(p => p.Category.Slug == queryParams.Category);
            }

            if (!string.IsNullOrEmpty(queryParams.Brand))
            {
                query = query.Where(p => p.Brand.Slug == queryParams.Brand);
            }

            if (queryParams.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= queryParams.MinPrice.Value);
            }

            if (queryParams.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(queryParams.Size))
            {
                query = query.Where(p => p.Variants.Any(v => v.Size == queryParams.Size && v.StockQuantity > 0));
            }

            if (!string.IsNullOrEmpty(queryParams.Color))
            {
                query = query.Where(p => p.Variants.Any(v => v.Color == queryParams.Color && v.StockQuantity > 0));
            }

            if (queryParams.OnSale == true)
            {
                query = query.Where(p => p.IsOnSale);
            }

            if (queryParams.Featured == true)
            {
                query = query.Where(p => p.IsFeatured);
            }

            // جستجو
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchTerm = queryParams.Search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm));
            }

            // مرتب‌سازی
            query = queryParams.SortBy?.ToLower() switch
            {
                "price" => queryParams.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.Price) : 
                    query.OrderBy(p => p.Price),
                "name" => queryParams.SortOrder == "desc" ? 
                    query.OrderByDescending(p => p.Name) : 
                    query.OrderBy(p => p.Name),
                "created" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedList<Product>(products, totalCount, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<bool> UpdateProductStockAsync(int productId, string size, string color, int quantity)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.Size == size && v.Color == color);

            if (variant == null) return false;

            variant.StockQuantity = quantity;
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}