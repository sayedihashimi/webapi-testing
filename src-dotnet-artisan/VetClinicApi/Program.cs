using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.Endpoints;
using VetClinicApi.Middleware;
using VetClinicApi.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<VetClinicDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=vetclinic.db"));

// Services
builder.Services.AddScoped<OwnerService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<VeterinarianService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<MedicalRecordService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<VaccinationService>();

// OpenAPI
builder.Services.AddOpenApi();

// JSON enum serialization as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Global error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Error handling middleware
app.UseExceptionHandler();

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}

// Map endpoints
app.MapOwnerEndpoints();
app.MapPetEndpoints();
app.MapVeterinarianEndpoints();
app.MapAppointmentEndpoints();
app.MapMedicalRecordEndpoints();
app.MapPrescriptionEndpoints();
app.MapVaccinationEndpoints();

await app.RunAsync();
