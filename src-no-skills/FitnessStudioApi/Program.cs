using FitnessStudioApi.Data;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Services;
using FitnessStudioApi.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IClassTypeService, ClassTypeService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI / Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Global error handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Fitness Studio API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();
