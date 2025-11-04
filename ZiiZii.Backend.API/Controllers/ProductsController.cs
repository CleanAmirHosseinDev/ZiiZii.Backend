// ZiiZii.Backend.API/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using ZiiZiiKids.Core.Entities;
using ZiiZiiKids.Core.Interfaces;

namespace ZiiZiiKids.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedList<Product>>>> GetProducts(
            [FromQuery] ProductQueryParams queryParams)
        {
            try
            {
                var products = await _productService.GetProductsAsync(queryParams);
                
                return Ok(new ApiResponse<PagedList<Product>>
                {
                    Success = true,
                    Data = products,
                    Message = "Products retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Product>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<Product>
                {
                    Success = true,
                    Data = product,
                    Message = "Product retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the product"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Product>>> CreateProduct(Product product)
        {
            try
            {
                var createdProduct = await _productService.CreateProductAsync(product);
                
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, 
                    new ApiResponse<Product>
                    {
                        Success = true,
                        Data = createdProduct,
                        Message = "Product created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the product"
                });
            }
        }

        [HttpPut("{id}/stock")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStock(
            int id, 
            [FromBody] UpdateStockRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductStockAsync(
                    id, request.Size, request.Color, request.Quantity);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product variant not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Stock updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating stock"
                });
            }
        }
    }

    public class UpdateStockRequest
    {
        public string Size { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}