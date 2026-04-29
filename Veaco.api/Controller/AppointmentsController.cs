using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // Feature 13: Book a service appointment
    [HttpPost]
    public async Task<IActionResult> BookAppointment(BookAppointmentDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);

        if (customer == null)
            return NotFound("Customer not found.");

        if (dto.VehicleId.HasValue)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId && v.CustomerId == dto.CustomerId);
            if (vehicle == null)
                return NotFound("Vehicle not found for this customer.");
        }

        if (dto.AppointmentDate <= DateTime.UtcNow)
            return BadRequest("Appointment date must be in the future.");

        if (string.IsNullOrWhiteSpace(dto.ServiceDescription))
            return BadRequest("Service description is required.");

        var appointment = new Appointment
        {
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            AppointmentDate = dto.AppointmentDate,
            ServiceDescription = dto.ServiceDescription,
            Status = "Pending"
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Appointment booked successfully.",
            AppointmentId = appointment.Id,
            appointment.AppointmentDate,
            appointment.Status
        });
    }

    // Feature 13: Get appointments for a customer
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerAppointments(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null)
            return NotFound("Customer not found.");

        var appointments = await _context.Appointments
            .Where(a => a.CustomerId == customerId)
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.ServiceDescription,
                a.Status,
                a.CreatedAt,
                Vehicle = a.Vehicle == null ? null : new { a.Vehicle.Id, a.Vehicle.VehicleNumber, a.Vehicle.Brand, a.Vehicle.Model }
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // Get all appointments (staff view)
    [HttpGet]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.ServiceDescription,
                a.Status,
                Customer = new { a.Customer.Id, a.Customer.FullName, a.Customer.Phone },
                Vehicle = a.Vehicle == null ? null : new { a.Vehicle.VehicleNumber, a.Vehicle.Brand, a.Vehicle.Model }
            })
            .ToListAsync();

        return Ok(appointments);
    }
}
