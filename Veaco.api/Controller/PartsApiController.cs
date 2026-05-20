using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTOs;
using Veaco.api.Model;
using Veaco.api.Services;

namespace Veaco.api.Controllers
{
    [ApiController]
    [Route("api/parts")]
    public class PartsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public PartsApiController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetParts()
        {
            var parts = await _context.VehicleParts
                .Include(p => p.Vendor)
                .Select(p => new PartDto
                {
                    Id = p.Id,
                    Name = p.PartName,
                    Category = p.Category,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    VendorName = p.Vendor != null ? p.Vendor.VendorName : null
                })
                .ToListAsync();

            return Ok(parts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPart(int id)
        {
            var part = await _context.VehicleParts
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (part == null)
                return NotFound();

            var dto = new PartDto
            {
                Id = part.Id,
                Name = part.PartName,
                Category = part.Category,
                Price = part.Price,
                StockQuantity = part.StockQuantity,
                VendorName = part.Vendor?.VendorName
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePart(CreatePartDto dto)
        {
            var part = new VehiclePart
            {
                PartName = dto.Name,
                Category = dto.Category,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                VendorId = dto.VendorId
            };

            _context.VehicleParts.Add(part);
            await _context.SaveChangesAsync();

            if (part.StockQuantity < 10)
            {
                await _emailService.SendLowStockNotificationAsync(
                    part.PartName,
                    part.StockQuantity
                );
            }

            return Ok(part);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePart(int id, UpdatePartDto dto)
        {
            var part = await _context.VehicleParts.FindAsync(id);

            if (part == null)
                return NotFound();

            part.PartName = dto.Name;
            part.Category = dto.Category;
            part.Price = dto.Price;
            part.StockQuantity = dto.StockQuantity;
            part.VendorId = dto.VendorId;

            await _context.SaveChangesAsync();

            if (part.StockQuantity < 10)
            {
                await _emailService.SendLowStockNotificationAsync(
                    part.PartName,
                    part.StockQuantity
                );
            }

            return Ok(part);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var part = await _context.VehicleParts.FindAsync(id);

            if (part == null)
                return NotFound();

            _context.VehicleParts.Remove(part);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
    }
}