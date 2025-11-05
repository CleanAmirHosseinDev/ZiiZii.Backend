using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Core.Entities;
using ZiiZii.Backend.Infrastructure.Data;

namespace ZiiZii.Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // محاسبه مجموع
                decimal totalAmount = 0;

                // ایجاد سفارش
                var order = new Order
                {
                    UserId = request.UserId.ToString(),
                    //ShippingAddress = request.ShippingAddress,
                    Status = "pending",
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                // افزودن آیتم‌ها
                foreach (var item in request.Items)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                    if (variant == null || variant.StockQuantity < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"موجودی کافی نیست. محصول: {item.VariantId}"
                        });
                    }

                    var orderItem = new OrderItem
                    {
                        ProductVariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = variant.Price
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;

                    // کاهش موجودی
                    variant.StockQuantity -= item.Quantity;
                }

                order.TotalAmount = totalAmount;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = order.Id,
                        totalAmount = order.TotalAmount
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ثبت سفارش"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { success = false, message = "سفارش پیدا نشد" });

            return Ok(new { success = true, data = order });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId.ToString())
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return Ok(new { success = true, data = orders });
        }
    }

    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }
}