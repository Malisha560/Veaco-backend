namespace Veaco.api.Model;

//  represents a staff member in  system 
public class Staff
{
  public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // this stores the hashed version
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
    // Automatically records when this staff member was registered
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}