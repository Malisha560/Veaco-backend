namespace Veaco.api.Model;

public class Appointment
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime AppointmentDate { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceReview> ServiceReviews { get; set; } = new List<ServiceReview>();
}
