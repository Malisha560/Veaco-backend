using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/notifications/low-stock
    // Returns all vehicle parts with stock below 10 units wala feature

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockParts()
    {
        // Fetch all parts where quantity has dropped below the minimum of 10
        var lowStockParts = await _context.VehicleParts
            .Where(p => p.StockQuantity < 10)
            .Select(p => new
            {
                p.Id,
                p.PartName,
                p.StockQuantity,
                p.Price,
                // Add a severity label so the frontend can color-code the alerts
                Severity = p.StockQuantity == 0 ? "Out of Stock" : "Low Stock"
            })
            .OrderBy(p => p.StockQuantity) // Show most critical (lowest stock) first
            .ToListAsync();

        return Ok(new
        {
            TotalLowStockParts = lowStockParts.Count,
            Parts = lowStockParts
        });
    }


    // GET api/notifications/overdue-credits
    //  Returns all customers with unpaid credit balance wala feature
    // overdue for more than 1 month


    [HttpGet("overdue-credits")]
    public async Task<IActionResult> GetOverdueCredits()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        // yo code le  customer IDs that have at least one invoice older than 1 month find garcha 

        var customerIdsWithOldInvoices = await _context.SalesInvoices
            .Where(i => i.InvoiceDate <= oneMonthAgo)
            .Select(i => i.CustomerId)
            .Distinct()
            .ToListAsync();

        // yo code le fetch customers who have a credit balance AND appear in the list above
        var overdueCustomers = await _context.Customers
            .Where(c => c.CreditBalance > 0 &&
                        customerIdsWithOldInvoices.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.CreditBalance
            })
            .OrderByDescending(c => c.CreditBalance) // Show highest debt first
            .ToListAsync();

        return Ok(new
        {
            TotalOverdueCustomers = overdueCustomers.Count,
            Customers = overdueCustomers
        });
    }


    // GET api/notifications/summary
    //Returns a quick count of low stock parts and overdue customers wala freature
    // Used by the frontend dashboard to show a notification badge/card
    [HttpGet("summary")]
    public async Task<IActionResult> GetNotificationSummary()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var lowStockItems = await _context.VehicleParts
            .Where(p => p.StockQuantity < 10)
            .Select(p => new
            {
                id = p.Id,
                partName = p.PartName,
                stockQuantity = p.StockQuantity,
                price = p.Price
            })
            .OrderBy(p => p.stockQuantity)
            .ToListAsync();

        var customerIdsWithOldInvoices = await _context.SalesInvoices
            .Where(i => i.InvoiceDate <= oneMonthAgo)
            .Select(i => i.CustomerId)
            .Distinct()
            .ToListAsync();

        var overdueCreditsCount = await _context.Customers
            .CountAsync(c => c.CreditBalance > 0 &&
                             customerIdsWithOldInvoices.Contains(c.Id));

        return Ok(new
        {
            lowStockItems = lowStockItems,
            lowStockCount = lowStockItems.Count,
            overdueCreditsCount = overdueCreditsCount,
            totalAlerts = lowStockItems.Count + overdueCreditsCount
        });
    }
}