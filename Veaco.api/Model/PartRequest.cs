namespace Veaco.api.Model;

public class PartRequest
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string PartName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
}
