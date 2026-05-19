using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Veace.api.Data;
using Veaco.api.Model;
using Veaco.api.Services;

namespace Veaco.api.Controller;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupFirstAdmin()
    {
        var adminExists = await _context.Staff.AnyAsync(s => s.Role == "Admin");

        if (adminExists)
            return BadRequest(new { Message = "An admin already exists." });

        var admin = new Staff
        {
            FullName = "Veaco Admin",
            Email = "veaco73@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("veaco123@!"),
            Role = "Admin",
            IsActive = true
        };

        _context.Staff.Add(admin);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "First admin created successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // 1. First check Staff/Admin table
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == dto.Email);

        if (staff != null)
        {
            if (!staff.IsActive)
                return Unauthorized(new { Message = "Staff account is inactive." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, staff.PasswordHash))
                return Unauthorized(new { Message = "Invalid password." });

            var token = GenerateStaffToken(staff);

            return Ok(new
            {
                message = "Login successful.",
                token,
                user = new
                {
                    staff.Id,
                    staff.FullName,
                    staff.Email,
                    staff.Role,
                    CustomerId = (int?)null
                }
            });
        }

        // 2. Then check Customer/AppUsers table
        var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (appUser == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, appUser.PasswordHash))
            return Unauthorized(new { Message = "Invalid email or password." });

        int? customerId = null;

        if (appUser.Role == "Customer")
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == appUser.Email);

            if (customer != null)
                customerId = customer.Id;
        }

        var customerToken = GenerateAppUserToken(appUser);

        return Ok(new
        {
            message = "Login successful.",
            token = customerToken,
            user = new
            {
                appUser.Id,
                appUser.FullName,
                appUser.Email,
                appUser.Role,
                CustomerId = customerId
            }
        });
    }

    private string GenerateStaffToken(Staff staff)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
            new Claim(ClaimTypes.Name, staff.FullName),
            new Claim(ClaimTypes.Email, staff.Email),
            new Claim(ClaimTypes.Role, staff.Role)
        };

        return BuildToken(claims);
    }

    private string GenerateAppUserToken(AppUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        return BuildToken(claims);
    }

    private string BuildToken(Claim[] claims)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}