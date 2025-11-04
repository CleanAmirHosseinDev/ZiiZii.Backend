// ZiiZii.Backend.Core/Interfaces/IProductService.cs

using ZiiZii.Backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZiiZii.Backend.Core.Interfaces
{
    public interface IProductService
    {
        Task<PagedList<Product>> GetProductsAsync(ProductQueryParams queryParams);
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductStockAsync(int productId, string size, string color, int quantity);
    }

    public class ProductQueryParams
    {
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? OnSale { get; set; }
        public bool? Featured { get; set; }
        public string Search { get; set; }
        public string SortBy { get; set; } = "created";
        public string SortOrder { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class PagedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}