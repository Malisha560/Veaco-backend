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
    public DbSet<Vendor> Vendors { get; set; }

    
    public DbSet<Part> Parts { get; set; }
    public DbSet<Veace.api.Models.PurchaseItem> PurchaseItems { get; set; }
    public DbSet<Veace.api.Models.PurchaseInvoice> PurchaseInvoices { get; set; }
}