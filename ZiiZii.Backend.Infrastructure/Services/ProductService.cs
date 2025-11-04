// ZiiZii.Backend.Infrastructure/Services/ProductService.cs
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Core.Entities;
using ZiiZii.Backend.Core.Interfaces;
using ZiiZii.Backend.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ZiiZii.Backend.Infrastructure.Services
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

            if (!string.IsNullOrEmpty(queryParams.Category))
            {
                query = query.Where(p => p.Category.Name == queryParams.Category);
            }

            if (!string.IsNullOrEmpty(queryParams.Brand))
            {
                query = query.Where(p => p.Brand.Name == queryParams.Brand);
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

            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var searchTerm = queryParams.Search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm));
            }

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
                .Include(p => p.Images)
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

        public async Task<Product> UpdateProductAsync(int id, Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.OriginalPrice = product.OriginalPrice;
            existingProduct.Description = product.Description;
            existingProduct.SKU = product.SKU;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.IsActive = product.IsActive;
            existingProduct.IsFeatured = product.IsFeatured;
            existingProduct.IsOnSale = product.IsOnSale;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductStockAsync(int productId, string size, string color, int quantity)
        {
            var variant = await _context.Set<ProductVariant>()
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.Size == size && v.Color == color);

            if (variant == null) return false;

            variant.StockQuantity = quantity;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}