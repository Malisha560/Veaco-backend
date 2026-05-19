using Veaco.api.Model;

namespace Veace.api.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PartId { get; set; }

        // Navigation to the Part that was purchased
        public VehiclePart? Part { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public int PurchaseInvoiceId { get; set; }

        // Navigation to the parent invoice
        public PurchaseInvoice? PurchaseInvoice { get; set; }
    }
}