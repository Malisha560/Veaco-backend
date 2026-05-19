using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/admin/reports")]
public class ReportsController : ControllerBase
{
    // Database access
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    // GET daily report
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var startOfDay = date.Date.ToUniversalTime();
        var endOfDay = startOfDay.AddDays(1);

        var invoices = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .Where(i => i.InvoiceDate >= startOfDay && i.InvoiceDate < endOfDay)
            .ToListAsync();

        // totals
        var totalSales = invoices.Sum(i => i.GrandTotal);
        var totalDiscount = invoices.Sum(i => i.DiscountAmount);
        var totalInvoices = invoices.Count;
        var totalItemsSold = invoices.SelectMany(i => i.Items).Sum(item => item.Quantity);

        return Ok(new
        {
            ReportType = "Daily",
            Date = date.Date.ToString("yyyy-MM-dd"),
            TotalInvoices = totalInvoices,
            TotalItemsSold = totalItemsSold,
            TotalDiscount = totalDiscount,
            TotalRevenue = totalSales,

            Invoices = invoices.Select(i => new
            {
                i.Id,
                Customer = i.Customer.FullName,
                i.InvoiceDate,
                i.SubTotal,
                i.DiscountAmount,
                i.GrandTotal,
                ItemCount = i.Items.Count
            })
        });
    }

    // GET monthly report
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);

        var invoices = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Where(i => i.InvoiceDate >= startOfMonth && i.InvoiceDate < endOfMonth)
            .ToListAsync();

        // totals
        var totalSales = invoices.Sum(i => i.GrandTotal);
        var totalDiscount = invoices.Sum(i => i.DiscountAmount);
        var totalInvoices = invoices.Count;
        var totalItemsSold = invoices.SelectMany(i => i.Items).Sum(item => item.Quantity);

        // group by day
        var dailyBreakdown = invoices
            .GroupBy(i => i.InvoiceDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Invoices = g.Count(),
                Revenue = g.Sum(i => i.GrandTotal)
            });

        return Ok(new
        {
            ReportType = "Monthly",
            Year = year,
            Month = month,
            TotalInvoices = totalInvoices,
            TotalItemsSold = totalItemsSold,
            TotalDiscount = totalDiscount,
            TotalRevenue = totalSales,
            DailyBreakdown = dailyBreakdown
        });
    }

    // GET yearly report
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
    {
        var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfYear = startOfYear.AddYears(1);

        var invoices = await _context.SalesInvoices
            .Include(i => i.Items)
            .Where(i => i.InvoiceDate >= startOfYear && i.InvoiceDate < endOfYear)
            .ToListAsync();

        // totals
        var totalSales = invoices.Sum(i => i.GrandTotal);
        var totalDiscount = invoices.Sum(i => i.DiscountAmount);
        var totalInvoices = invoices.Count;
        var totalItemsSold = invoices.SelectMany(i => i.Items).Sum(item => item.Quantity);

        // group by month
        var monthlyBreakdown = invoices
            .GroupBy(i => i.InvoiceDate.Month)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Month = new DateTime(year, g.Key, 1).ToString("MMMM"),
                MonthNumber = g.Key,
                Invoices = g.Count(),
                Revenue = g.Sum(i => i.GrandTotal),
                Discount = g.Sum(i => i.DiscountAmount)
            });

        return Ok(new
        {
            ReportType = "Yearly",
            Year = year,
            TotalInvoices = totalInvoices,
            TotalItemsSold = totalItemsSold,
            TotalDiscount = totalDiscount,
            TotalRevenue = totalSales,
            MonthlyBreakdown = monthlyBreakdown
        });
    }

    // GET summary (today / month / year)
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var now = DateTime.UtcNow;

        var todayStart = now.Date.ToUniversalTime();
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var allInvoices = await _context.SalesInvoices.ToListAsync();

        return Ok(new
        {
            Today = new
            {
                Revenue = allInvoices.Where(i => i.InvoiceDate >= todayStart).Sum(i => i.GrandTotal),
                Invoices = allInvoices.Count(i => i.InvoiceDate >= todayStart)
            },
            ThisMonth = new
            {
                Revenue = allInvoices.Where(i => i.InvoiceDate >= monthStart).Sum(i => i.GrandTotal),
                Invoices = allInvoices.Count(i => i.InvoiceDate >= monthStart)
            },
            ThisYear = new
            {
                Revenue = allInvoices.Where(i => i.InvoiceDate >= yearStart).Sum(i => i.GrandTotal),
                Invoices = allInvoices.Count(i => i.InvoiceDate >= yearStart)
            }
        });
    }
}