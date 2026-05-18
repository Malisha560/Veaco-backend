using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;


namespace Veaco.api.Controller;

[ApiController]
[Route("api/customer-reports")]
public class CustomerReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("regular-customers")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        var regularCustomers = await _context.SalesInvoices
            .Include(i => i.Customer)
            .GroupBy(i => new
            {
                i.CustomerId,
                i.Customer.FullName,
                i.Customer.Phone,
                i.Customer.Email
            })
            .Where(g => g.Count() >= 2)
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.FullName,
                g.Key.Phone,
                g.Key.Email,
                TotalPurchases = g.Count(),
                TotalSpent = g.Sum(i => i.GrandTotal)
            })
            .OrderByDescending(c => c.TotalPurchases)
            .ToListAsync();

        return Ok(regularCustomers);
    }

    [HttpGet("high-spenders")]
    public async Task<IActionResult> GetHighSpenders()
    {
        var highSpenders = await _context.SalesInvoices
            .Include(i => i.Customer)
            .GroupBy(i => new
            {
                i.CustomerId,
                i.Customer.FullName,
                i.Customer.Phone,
                i.Customer.Email
            })
            .Where(g => g.Sum(i => i.GrandTotal) >= 5000)
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.FullName,
                g.Key.Phone,
                g.Key.Email,
                TotalSpent = g.Sum(i => i.GrandTotal),
                TotalPurchases = g.Count()
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToListAsync();

        return Ok(highSpenders);
    }

    [HttpGet("pending-credits")]
    public async Task<IActionResult> GetPendingCredits()
    {
        var pendingCredits = await _context.Customers
            .Where(c => c.CreditBalance > 0)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Phone,
                c.Email,
                c.CreditBalance
            })
            .OrderByDescending(c => c.CreditBalance)
            .ToListAsync();

        return Ok(pendingCredits);
    }
}