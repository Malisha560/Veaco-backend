using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetCustomerDetails(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .Include(c => c.SalesInvoices)
            .ThenInclude(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound("Customer not found.");

        return Ok(customer);
    }

    [HttpGet("{id}/purchase-history")]
    public async Task<IActionResult> GetPurchaseHistory(int id)
    {
        var history = await _context.SalesInvoices
            .Where(i => i.CustomerId == id)
            .Include(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        return Ok(history);
    }
}