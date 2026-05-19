using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Veaco.api.Model;

namespace Veaco.api.Services;


public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    // Generates a JWT token for a given staff member
    // The token contains the staff's ID, email, and role
    public string GenerateToken(Staff staff)
    {
        // Claims are pieces of info  inside the token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
            new Claim(ClaimTypes.Email, staff.Email),
            new Claim(ClaimTypes.Role, staff.Role),
            new Claim(ClaimTypes.Name, staff.FullName)
        };

        // Use the secret key from appsettings.json to sign the token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build the token with an expiry of 8 hours
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        // Return the token as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}