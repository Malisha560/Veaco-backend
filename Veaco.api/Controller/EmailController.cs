using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.Services;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public EmailController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    
    // POST api/email/send-invoice/{invoiceId}
    // Staff sends an invoice email to the customer wala feature
    [HttpPost("send-invoice/{invoiceId}")]
    public async Task<IActionResult> SendInvoiceEmail(int invoiceId)
    {
        // Fetch the invoice along with customer info and all purchased items
        var invoice = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return NotFound("Invoice not found.");

        // Make sure the customer has an email address on file
        if (string.IsNullOrWhiteSpace(invoice.Customer.Email))
            return BadRequest("Customer does not have an email address on file.");

        // Build a readable list of items
        var itemDescriptions = invoice.Items.Select(item =>
            $"{item.VehiclePart.PartName} x{item.Quantity} — Rs. {item.LineTotal:F2}"
        ).ToList();

        try
        {
            await _emailService.SendInvoiceEmailAsync(
                invoice.Customer.Email,
                invoice.Customer.FullName,
                invoice.Id,
                invoice.SubTotal,
                invoice.DiscountAmount,
                invoice.GrandTotal,
                itemDescriptions
            );

            return Ok(new { Message = $"Invoice #{invoiceId} sent successfully to {invoice.Customer.Email}." });
        }
        catch (Exception ex)
        {
            // Return the error so it is easy to debug in Swagger
            return StatusCode(500, new { Message = "Failed to send email.", Error = ex.Message });
        }
    }


    // POST api/email/notify-low-stock
    // Checks all vehicle parts and sends the admin wala feature

    [HttpPost("notify-low-stock")]
    public async Task<IActionResult> NotifyLowStock()
    {
        // Find every part where stock has dropped below the minimum of 10
        var lowStockParts = await _context.VehicleParts
            .Where(p => p.StockQuantity < 10)
            .ToListAsync();

        if (!lowStockParts.Any())
            return Ok(new { Message = "All parts are sufficiently stocked. No alerts sent." });

        var sentAlerts = new List<string>();

        // Send a separate email alert to admin for each low stock part
        foreach (var part in lowStockParts)
        {
            try
            {
                await _emailService.SendLowStockNotificationAsync(part.PartName, part.StockQuantity);
                sentAlerts.Add($"{part.PartName} ({part.StockQuantity} units)");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to send alert for {part.PartName}.", Error = ex.Message });
            }
        }

        return Ok(new
        {
            Message = $"{sentAlerts.Count} low stock alert(s) sent to admin.",
            Parts = sentAlerts
        });
    }

    
    // POST api/email/send-credit-reminders
    // Finds all customers with an unpaid credit balance wala feature

    [HttpPost("send-credit-reminders")]
    public async Task<IActionResult> SendCreditReminders()
    {
        // Calculate the cutoff date — anything older than this is overdue
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        // Find customers who have a credit balance AND have an invoice older than 1 month
        var overdueCustomers = await _context.Customers
            .Where(c => c.CreditBalance > 0 &&
                        c.SalesInvoices.Any(i => i.InvoiceDate <= oneMonthAgo))
            .ToListAsync();

        if (!overdueCustomers.Any())
            return Ok(new { Message = "No customers with overdue credit found. No reminders sent." });

        var remindersSent = new List<string>();

        // Send a reminder email to each overdue customer
        foreach (var customer in overdueCustomers)
        {
            // Skip customers who don't have an email address on file
            if (string.IsNullOrWhiteSpace(customer.Email))
                continue;

            try
            {
                await _emailService.SendCreditReminderEmailAsync(
                    customer.Email,
                    customer.FullName,
                    customer.CreditBalance
                );
                remindersSent.Add($"{customer.FullName} ({customer.Email})");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Failed to send reminder to {customer.FullName}.", Error = ex.Message });
            }
        }

        return Ok(new
        {
            Message = $"{remindersSent.Count} credit reminder(s) sent.",
            Customers = remindersSent
        });
    }
}