namespace Veaco.api.Model;

public class VehiclePart
{
    public int Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}