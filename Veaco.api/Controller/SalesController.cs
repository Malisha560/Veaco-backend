using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-invoice")]
    public async Task<IActionResult> CreateInvoice(CreateSalesInvoiceDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);

        if (customer == null)
            return NotFound("Customer not found.");

        if (dto.Items.Count == 0)
            return BadRequest("Invoice must contain at least one item.");

        decimal subTotal = 0;
        var invoiceItems = new List<SalesInvoiceItem>();

        foreach (var item in dto.Items)
        {
            var part = await _context.VehicleParts.FindAsync(item.VehiclePartId);

            if (part == null)
                return NotFound($"Part with ID {item.VehiclePartId} not found.");

            if (item.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            if (part.StockQuantity < item.Quantity)
                return BadRequest($"Not enough stock for {part.PartName}.");

            decimal lineTotal = part.Price * item.Quantity;
            subTotal += lineTotal;

            part.StockQuantity -= item.Quantity;

            invoiceItems.Add(new SalesInvoiceItem
            {
                VehiclePartId = part.Id,
                Quantity = item.Quantity,
                UnitPrice = part.Price,
                LineTotal = lineTotal
            });
        }

        decimal discountAmount = subTotal > 5000 ? subTotal * 0.10m : 0;
        decimal grandTotal = subTotal - discountAmount;

        var invoice = new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            GrandTotal = grandTotal,
            Items = invoiceItems
        };

        _context.SalesInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Sales invoice created successfully.",
            InvoiceId = invoice.Id,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            GrandTotal = invoice.GrandTotal
        });
    }

    [HttpGet("invoice/{id}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound("Invoice not found.");

        return Ok(invoice);
    }
}