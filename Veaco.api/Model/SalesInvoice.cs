namespace Veaco.api.Model;

public class SalesInvoice
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
}