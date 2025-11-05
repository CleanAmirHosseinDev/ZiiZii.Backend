using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZiiZii.Backend.Infrastructure.Data;

namespace ZiiZii.Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Slug,
                    b.Logo,
                    ProductCount = b.Products.Count(p => p.IsActive)
                })
                .ToListAsync();

            return Ok(new { success = true, data = brands });
        }
    }
}