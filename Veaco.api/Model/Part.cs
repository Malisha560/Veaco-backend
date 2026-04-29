using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace veaco.Models
{
    public class Part
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }
    }
}