namespace Veaco.api.DTO;

public class CreateSalesInvoiceDto
{
    public int CustomerId { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}

public class SalesInvoiceItemDto
{
    public int VehiclePartId { get; set; }
    public int Quantity { get; set; }
}