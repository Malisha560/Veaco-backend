namespace Veaco.api.Model;

public class ServiceReview
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
}
