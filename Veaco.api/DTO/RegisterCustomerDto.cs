namespace Veaco.api.DTO;

public class RegisterCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

    public RegisterVehicleDto? Vehicle { get; set; }
}

public class RegisterVehicleDto
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
