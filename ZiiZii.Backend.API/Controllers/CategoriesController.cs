using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Infrastructure.Data;

namespace ZiiZii.Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive && c.ParentId == null)
                .Include(c => c.Children)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.Image,
                    c.Description,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    Children = c.Children.Select(child => new
                    {
                        child.Id,
                        child.Name,
                        child.Slug
                    })
                })
                .ToListAsync();

            return Ok(new { success = true, data = categories });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _context.Categories
                .Where(c => c.Slug == slug && c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.Description,
                    c.Image,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { success = false, message = "Category not found" });

            return Ok(new { success = true, data = category });
        }
    }
}