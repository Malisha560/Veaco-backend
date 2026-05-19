using Microsoft.EntityFrameworkCore;

using Veaco.api.Model;
using Veaco.Api.Models;

namespace Veace.api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehiclePart> VehicleParts { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }

    // Hrithik features
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<PartRequest> PartRequests { get; set; }
    public DbSet<ServiceReview> ServiceReviews { get; set; }

    // Aayush features
    public DbSet<Staff> Staff { get; set; }

    // Malisha auth system
    public DbSet<AppUser> AppUsers { get; set; }

    // Bijem features
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<Veace.api.Models.PurchaseItem> PurchaseItems { get; set; }
    public DbSet<Veace.api.Models.PurchaseInvoice> PurchaseInvoices { get; set; }
}