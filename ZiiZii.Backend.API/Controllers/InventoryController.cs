using Microsoft.AspNetCore.Mvc;
using ZiiZii.Backend.Core.Interfaces;

namespace ZiiZii.Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request)
        {
            var result = await _inventoryService.AdjustStockAsync(
                request.VariantId,
                request.Quantity,
                request.Reason,
                request.Note
            );

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            var alerts = await _inventoryService.GetLowStockAlertsAsync();
            return Ok(new { success = true, data = alerts });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            var summary = await _inventoryService.GetStockSummaryAsync();
            return Ok(new { success = true, data = summary });
        }

        [HttpGet("history/{variantId}")]
        public async Task<IActionResult> GetInventoryHistory(int variantId)
        {
            var history = await _inventoryService.GetInventoryHistoryAsync(variantId);
            return Ok(new { success = true, data = history });
        }
    }

    public class AdjustStockRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
    }
}