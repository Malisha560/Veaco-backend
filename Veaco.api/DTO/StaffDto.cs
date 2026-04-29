namespace Veaco.api.DTO;
// Used when registering a new staff member
public class CreateStaffDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Plain text password from the request hash it before saving to the database
    public string Password { get; set; } = string.Empty;

    // Either Admin or Staff if nothing then defaults to Staff if not provided
    public string Role { get; set; } = "Staff";
}

public class UpdateStaffRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class UpdateStaffDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Admin can activate or deactivate a staff member using this field
    public bool IsActive { get; set; }
}