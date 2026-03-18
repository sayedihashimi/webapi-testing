using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.Endpoints;
using VetClinicApi.Middleware;
using VetClinicApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<VetClinicDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JSON enum serialization as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI + Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Error handling
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Services
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IVaccinationService, VaccinationService>();
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Error handling middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

// OpenAPI + Swagger UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapOwnerEndpoints();
app.MapPetEndpoints();
app.MapVeterinarianEndpoints();
app.MapAppointmentEndpoints();
app.MapMedicalRecordEndpoints();
app.MapPrescriptionEndpoints();
app.MapVaccinationEndpoints();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    db.Database.Migrate();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
