using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

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

    // Feature 6: Staff registers a new customer with optional vehicle details
    [HttpPost("register")]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Phone) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Full name, phone, email, and password are required.");
        }

        var emailChecker = new EmailAddressAttribute();

        if (!emailChecker.IsValid(dto.Email))
        {
            return BadRequest("Please enter a valid email address.");
        }

        var existingUser = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
            return BadRequest("An account with this email already exists.");

        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == dto.Email);

        if (existingCustomer != null)
            return BadRequest("Customer with this email already exists.");

        var customer = new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        };

        if (dto.Vehicle != null)
        {
            customer.Vehicles.Add(new Vehicle
            {
                VehicleNumber = dto.Vehicle.VehicleNumber,
                Brand = dto.Vehicle.Brand,
                Model = dto.Vehicle.Model
            });
        }

        _context.Customers.Add(customer);

        var user = new AppUser
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Role = "Customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.AppUsers.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Customer registered successfully.",
            CustomerId = customer.Id,
            UserId = user.Id,
            customer.FullName,
            VehiclesAdded = customer.Vehicles.Count
        });
    }
    // Feature 12: Customer self-registers
    [HttpPost("self-register")]
    public async Task<IActionResult> SelfRegister(RegisterCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Phone) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Full name, phone, email, and password are required.");
        }

        var existingUser = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
            return BadRequest("An account with this email already exists.");

        var customer = new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        };

        if (dto.Vehicle != null)
        {
            customer.Vehicles.Add(new Vehicle
            {
                VehicleNumber = dto.Vehicle.VehicleNumber,
                Brand = dto.Vehicle.Brand,
                Model = dto.Vehicle.Model
            });
        }

        _context.Customers.Add(customer);

        var user = new AppUser
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Role = "Customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.AppUsers.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Registration successful. Welcome to Veaco!",
            CustomerId = customer.Id,
            UserId = user.Id,
            customer.FullName
        });
    }

    // Feature 12: Update customer profile
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound("Customer not found.");

        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Profile updated successfully.", customer.FullName });
    }

    // Feature 12: Add a vehicle to customer's profile
    [HttpPost("{id}/vehicles")]
    public async Task<IActionResult> AddVehicle(int id, AddVehicleDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound("Customer not found.");

        var vehicle = new Vehicle
        {
            CustomerId = id,
            VehicleNumber = dto.VehicleNumber,
            Brand = dto.Brand,
            Model = dto.Model
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Vehicle added successfully.", VehicleId = vehicle.Id });
    }

    // Feature 12: Update a vehicle
    [HttpPut("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> UpdateVehicle(int id, int vehicleId, AddVehicleDto dto)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == id);

        if (vehicle == null)
            return NotFound("Vehicle not found.");

        vehicle.VehicleNumber = dto.VehicleNumber;
        vehicle.Brand = dto.Brand;
        vehicle.Model = dto.Model;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Vehicle updated successfully." });
    }

    // Feature 12: Delete a vehicle
    [HttpDelete("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(int id, int vehicleId)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == id);

        if (vehicle == null)
            return NotFound("Vehicle not found.");

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Vehicle removed successfully." });
    }

    // Feature 8 (existing): Get customer details with vehicles and invoices
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

    // Feature 14: View purchase/service history (invoices + appointments)
    [HttpGet("{id}/purchase-history")]
    public async Task<IActionResult> GetPurchaseHistory(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound("Customer not found.");

        var invoices = await _context.SalesInvoices
            .Where(i => i.CustomerId == id)
            .Include(i => i.Items)
            .ThenInclude(item => item.VehiclePart)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var appointments = await _context.Appointments
            .Where(a => a.CustomerId == id)
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.ServiceDescription,
                a.Status,
                Vehicle = a.Vehicle == null ? null : new { a.Vehicle.VehicleNumber, a.Vehicle.Brand, a.Vehicle.Model }
            })
            .ToListAsync();

        return Ok(new
        {
            Invoices = invoices,
            Appointments = appointments
        });
    }

// Get all customers
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _context.Customers
            .Include(c => c.Vehicles)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Phone,
                c.Email,
                Vehicles = c.Vehicles.Select(v => new
                {
                    v.Id,
                    v.VehicleNumber,
                    v.Brand,
                    v.Model
                })
            })
            .ToListAsync();

        return Ok(customers);
    }

// Feature 10: Search customers by vehicle number, phone, ID, or name
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Customer>>> SearchCustomers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required.");

        query = query.ToLower();

        var customers = await _context.Customers
            .Include(c => c.Vehicles)
            .Where(c =>
                c.FullName.ToLower().Contains(query) ||
                c.Phone.Contains(query) ||
                c.Id.ToString() == query ||
                c.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(query))
            )
            .ToListAsync();

        return Ok(customers);
    }
}