namespace Veaco.api.Model;

public class SalesInvoiceItem
{
    public int Id { get; set; }

    public int SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;

    public int VehiclePartId { get; set; }
    public VehiclePart VehiclePart { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}