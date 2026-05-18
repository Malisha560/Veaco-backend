using System.ComponentModel.DataAnnotations;

namespace Veaco.Api.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required]
        public string VendorName { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }
    }
}