using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/part-requests")]
public class PartRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PartRequestsController(AppDbContext context)
    {
        _context = context;
    }

    // Feature 13: Request an unavailable part
    [HttpPost]
    public async Task<IActionResult> CreatePartRequest(CreatePartRequestDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);

        if (customer == null)
            return NotFound("Customer not found.");

        if (string.IsNullOrWhiteSpace(dto.PartName))
            return BadRequest("Part name is required.");

        var request = new PartRequest
        {
            CustomerId = dto.CustomerId,
            PartName = dto.PartName,
            Notes = dto.Notes,
            Status = "Pending"
        };

        _context.PartRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Part request submitted successfully.",
            RequestId = request.Id,
            request.PartName,
            request.Status
        });
    }

    // Get part requests for a customer
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerPartRequests(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null)
            return NotFound("Customer not found.");

        var requests = await _context.PartRequests
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.RequestDate)
            .Select(r => new
            {
                r.Id,
                r.PartName,
                r.Notes,
                r.RequestDate,
                r.Status
            })
            .ToListAsync();

        return Ok(requests);
    }

    // Get all part requests (staff view)
    [HttpGet]
    public async Task<IActionResult> GetAllPartRequests()
    {
        var requests = await _context.PartRequests
            .Include(r => r.Customer)
            .OrderByDescending(r => r.RequestDate)
            .Select(r => new
            {
                r.Id,
                r.PartName,
                r.Notes,
                r.RequestDate,
                r.Status,
                Customer = new { r.Customer.Id, r.Customer.FullName, r.Customer.Phone }
            })
            .ToListAsync();

        return Ok(requests);
    }
}
