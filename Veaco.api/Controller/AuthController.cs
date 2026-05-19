using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veace.api.Data;
using Veaco.api.Model;
using Veaco.api.Services;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    // POST api/auth/setup
    // Creates the very first admin only works if no admin exists yet
    // After this, new admins can only be created by an existing admin via StaffController
    [HttpPost("setup")]
    public async Task<IActionResult> SetupFirstAdmin()
    {
        // Check if any admin already exists in the system
        var adminExists = await _context.Staff.AnyAsync(s => s.Role == "Admin");

        if (adminExists)
            return BadRequest(new { Message = "An admin already exists. Use the staff registration endpoint to add more." });

        // Create the first admin with the hardcoded credentials
        var admin = new Staff
        {
            FullName = "Veaco Admin",
            Email = "veaco73@gmail.com",
            // Hash the password before saving — never store plain text passwords
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("veaco123@!"),
            Role = "Admin",
            IsActive = true
        };

        _context.Staff.Add(admin);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "First admin created successfully. You can now log in." });
    }

    // POST api/auth/login
    // Staff or Admin logs in with email and password
    // Returns a JWT token if credentials are correct
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Find the staff member by email
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == dto.Email);

        // Check if staff exists and is active
        if (staff == null || !staff.IsActive)
            return Unauthorized(new { Message = "Invalid email or account is inactive." });

        // Verify the password against the stored hash
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, staff.PasswordHash))
            return Unauthorized(new { Message = "Invalid password." });

        // Generate a JWT token for this staff member
        var token = _jwtService.GenerateToken(staff);

        return Ok(new
        {
            Message = "Login successful.",
            Token = token,
            Staff = new
            {
                staff.Id,
                staff.FullName,
                staff.Email,
                staff.Role
            }
        });
    }
}

// Simple DTO for the login request body
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}