using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.DTO;
using Veaco.api.Model;

namespace Veaco.api.Controller;

//  tells ASP.NET this class handles API requests
[ApiController]

// Base route for all endpoints in this controller
[Route("api/admin/staff")]
public class StaffController : ControllerBase
{
    // Database context to interact with DB
    private readonly AppDbContext _context;

    // Constructor to inject DB context
    public StaffController(AppDbContext context)
    {
        _context = context;
    }
   
    // POST api/admin/staff/register
    // Used to create a new staff account
    [HttpPost("register")]
    public async Task<IActionResult> RegisterStaff(CreateStaffDto dto)
    {
        // Check required fields
        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("FullName, Email, and Password are required.");

        // Allow only Admin or Staff roles
        var validRoles = new[] { "Admin", "Staff" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest("Role must be either 'Admin' or 'Staff'.");

        // Check if email already exists
        var exists = await _context.Staff.AnyAsync(s => s.Email == dto.Email);
        if (exists)
            return Conflict("A staff member with this email already exists.");

        // Create new staff object
        var staff = new Staff
        {
            FullName = dto.FullName,
            Email = dto.Email,

            // Password is hashed before saving (security)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

            Role = dto.Role,
            IsActive = true,

            // Store creation time
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();

        // Return success response
        return Ok(new
        {
            Message = "Staff registered successfully.",
            StaffId = staff.Id,
            staff.FullName,
            staff.Email,
            staff.Role
        });
    }

    // GET api/admin/staff
    // Returns all staff (latest first)
    [HttpGet]
    public async Task<IActionResult> GetAllStaff()
    {
        // Select only needed fields (hide password)
        var staffList = await _context.Staff
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.FullName,
                s.Email,
                s.Role,
                s.IsActive,
                s.CreatedAt
            })
            .ToListAsync();

        return Ok(staffList);
    }

    // GET api/admin/staff/{id}
    // Get single staff by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStaffById(int id)
    {
        var staff = await _context.Staff.FindAsync(id);

        // If not found → 404
        if (staff == null)
            return NotFound("Staff member not found.");

        return Ok(new
        {
            staff.Id,
            staff.FullName,
            staff.Email,
            staff.Role,
            staff.IsActive,
            staff.CreatedAt
        });
    }

    // PUT api/admin/staff/{id}/role
    // Update only role (Admin ↔ Staff)
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateStaffRole(int id, UpdateStaffRoleDto dto)
    {
        // Validate role
        var validRoles = new[] { "Admin", "Staff" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest("Role must be either 'Admin' or 'Staff'.");

        var staff = await _context.Staff.FindAsync(id);
        if (staff == null)
            return NotFound("Staff member not found.");

        // Update role
        staff.Role = dto.Role;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Role updated to '{dto.Role}' successfully." });
    }

    // PUT api/admin/staff/{id}
    // Update full staff details
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, UpdateStaffDto dto)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null)
            return NotFound("Staff member not found.");

        // Validate role again
        var validRoles = new[] { "Admin", "Staff" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest("Role must be either 'Admin' or 'Staff'.");

        // Check email is not used by someone else
        var emailTaken = await _context.Staff
            .AnyAsync(s => s.Email == dto.Email && s.Id != id);
        if (emailTaken)
            return Conflict("Another staff member already uses this email.");

        // Apply updates
        staff.FullName = dto.FullName;
        staff.Email = dto.Email;
        staff.Role = dto.Role;
        staff.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Staff updated successfully." });
    }

    // DELETE api/admin/staff/{id}
    // Permanently delete staff
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null)
            return NotFound("Staff member not found.");

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Staff member deleted successfully." });
    }
}