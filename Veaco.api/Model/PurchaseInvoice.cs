using System;
using System.Collections.Generic;

namespace Veace.api.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseItem>? Items { get; set; } = new List<PurchaseItem>();
    }
}
