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

        // ✅ CREATE PURCHASE INVOICE + UPDATE STOCK
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(PurchaseInvoice invoice)
        {
            if (invoice.Items == null || !invoice.Items.Any())
                return BadRequest("Invoice must contain items.");

            decimal total = 0;

            foreach (var item in invoice.Items)
            {
                var part = await _context.Parts.FindAsync(item.PartId);

                if (part == null)
                    return BadRequest($"Part with ID {item.PartId} not found.");

                // 🔥 STOCK UPDATE (IMPORTANT FOR MARKS)
                part.StockQuantity += item.Quantity;

                // 🔥 CALCULATE TOTAL
                total += item.Quantity * item.Price;
            }

            invoice.TotalAmount = total;

            _context.PurchaseInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Purchase Invoice Created Successfully",
                totalAmount = total
            });
        }

        // ✅ GET ALL INVOICES
        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.PurchaseInvoices
                .Include(i => i.Items)
                .ToListAsync();

            return Ok(invoices);
        }

        // ✅ GET SINGLE INVOICE
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _context.PurchaseInvoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }
    }
}