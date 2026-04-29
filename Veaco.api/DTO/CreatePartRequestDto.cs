namespace Veaco.api.DTO;

public class CreatePartRequestDto
{
    public int CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
