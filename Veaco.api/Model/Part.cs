using Veaco.Api.Models;

namespace Veaco.api.Model
{
    public class Part
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;

        public string? Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public int? VendorId { get; set; }

        public Vendor? Vendor { get; set; }
    }
}