namespace Veace.api.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PartId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public int PurchaseInvoiceId { get; set; }
    }
}