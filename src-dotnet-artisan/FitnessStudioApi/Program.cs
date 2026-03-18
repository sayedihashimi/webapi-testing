using System.Text.Json.Serialization;
using FitnessStudioApi.Data;
using FitnessStudioApi.Endpoints;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<StudioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=FitnessStudio.db"));

// JSON enum serialization
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI
builder.Services.AddOpenApi();

// Global error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Application services
builder.Services.AddScoped<MembershipPlanService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<MembershipService>();
builder.Services.AddScoped<InstructorService>();
builder.Services.AddScoped<ClassTypeService>();
builder.Services.AddScoped<ClassScheduleService>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map all endpoints
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapMembershipEndpoints();
app.MapInstructorEndpoints();
app.MapClassTypeEndpoints();
app.MapClassScheduleEndpoints();
app.MapBookingEndpoints();

app.Run();
