using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Infrastructure.Data;

namespace ZiiZii.Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new { success = true, data = new List<object>() });

            var searchTerm = q.ToLower();

            var products = await _context.Products
                .Where(p => p.IsActive &&
                    (p.Name.ToLower().Contains(searchTerm) ||
                     p.Description.ToLower().Contains(searchTerm)))
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    MainImage = p.Images.FirstOrDefault(i => i.IsMain).ImageUrl,
                    Category = p.Category.Name
                })
                .ToListAsync();

            return Ok(new { success = true, data = products });
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new { success = true, data = new List<string>() });

            var searchTerm = q.ToLower();

            var suggestions = await _context.Products
                .Where(p => p.IsActive && p.Name.ToLower().Contains(searchTerm))
                .Select(p => p.Name)
                .Take(5)
                .ToListAsync();

            return Ok(new { success = true, data = suggestions });
        }
    }
}