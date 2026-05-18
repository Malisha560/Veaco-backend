using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/service-reviews")]
public class ServiceReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceReviewsController(AppDbContext context)
    {
        _context = context;
    }

    // Feature 13: Submit a service review
    [HttpPost]
    public async Task<IActionResult> CreateReview(CreateServiceReviewDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);

        if (customer == null)
            return NotFound("Customer not found.");

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        if (dto.AppointmentId.HasValue)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.CustomerId == dto.CustomerId);
            if (appointment == null)
                return NotFound("Appointment not found for this customer.");
        }

        var review = new ServiceReview
        {
            CustomerId = dto.CustomerId,
            AppointmentId = dto.AppointmentId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.ServiceReviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Review submitted successfully.",
            ReviewId = review.Id,
            review.Rating
        });
    }

    // Get all reviews (public)
    [HttpGet]
    public async Task<IActionResult> GetAllReviews()
    {
        var reviews = await _context.ServiceReviews
            .Include(r => r.Customer)
            .Include(r => r.Appointment)
            .OrderByDescending(r => r.ReviewDate)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.ReviewDate,
                CustomerName = r.Customer.FullName,
                ServiceDescription = r.Appointment != null ? r.Appointment.ServiceDescription : null
            })
            .ToListAsync();

        return Ok(reviews);
    }

    // Get reviews by customer
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerReviews(int customerId)
    {
        var reviews = await _context.ServiceReviews
            .Where(r => r.CustomerId == customerId)
            .Include(r => r.Appointment)
            .OrderByDescending(r => r.ReviewDate)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.ReviewDate,
                ServiceDescription = r.Appointment != null ? r.Appointment.ServiceDescription : null
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
