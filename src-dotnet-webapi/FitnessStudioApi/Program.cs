using System.Text.Json.Serialization;
using FitnessStudioApi.Data;
using FitnessStudioApi.Endpoints;
using FitnessStudioApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JSON enum serialization as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI & Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Error handling
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Service registrations
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IClassTypeService, ClassTypeService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

// Error handling middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

// OpenAPI & Swagger UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenith Fitness Studio API v1");
        options.RoutePrefix = string.Empty;
    });
}

// Map endpoints
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapMembershipEndpoints();
app.MapInstructorEndpoints();
app.MapClassTypeEndpoints();
app.MapClassScheduleEndpoints();
app.MapBookingEndpoints();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db);
}

app.Run();
