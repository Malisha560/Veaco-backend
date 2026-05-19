using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veace.api.Models;

namespace Veace.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchaseController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE PURCHASE ITEMS + UPDATE STOCK
        [HttpPost("items")]
        public async Task<IActionResult> CreatePurchaseItems(List<PurchaseItem> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Purchase must contain items.");

            decimal total = 0;

            foreach (var item in items)
            {
                var part = await _context.Parts.FindAsync(item.PartId);

                if (part == null)
                    return BadRequest($"Part with ID {item.PartId} not found.");

                // UPDATE STOCK
                part.StockQuantity += item.Quantity;

                // CALCULATE TOTAL
                total += item.Quantity * item.Price;
            }

            // Create a purchase invoice and attach items
            var invoice = new PurchaseInvoice
            {
                InvoiceDate = DateTime.UtcNow,
                TotalAmount = total,
                Items = items
            };

            _context.PurchaseInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchase Items Created Successfully",
                totalAmount = total,
                itemsCount = items.Count
            });
        }

        // GET ALL PURCHASE ITEMS
        [HttpGet("items")]
        public async Task<IActionResult> GetPurchaseItems()
        {
            var items = await _context.PurchaseItems
                .Include(i => i.Part)
                .ToListAsync();

            return Ok(items);
        }

        // GET PURCHASE ITEM BY ID
        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetPurchaseItem(int id)
        {
            var item = await _context.PurchaseItems
                .Include(i => i.Part)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // GET PURCHASE ITEMS BY PART
        [HttpGet("items/part/{partId}")]
        public async Task<IActionResult> GetPurchaseItemsByPart(int partId)
        {
            var items = await _context.PurchaseItems
                .Include(i => i.Part)
                .Where(i => i.PartId == partId)
                .ToListAsync();

            return Ok(items);
        }
    }
}