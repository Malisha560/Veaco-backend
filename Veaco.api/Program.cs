using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Veace.api.Data;
using Veaco.api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the EmailService so it can be injected into any controller that needs it
builder.Services.AddScoped<EmailService>();

// Add CORS so the React frontend on localhost:5173 can talk to this backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular reference errors when returning related data (e.g. Customer → Invoices → Customer)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Connect to PostgreSQL using the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Apply the CORS policy before authorization so frontend requests are not blocked
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();