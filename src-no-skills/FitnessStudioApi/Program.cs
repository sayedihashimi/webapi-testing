using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<MembershipPlanService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<MembershipService>();
builder.Services.AddScoped<InstructorService>();
builder.Services.AddScoped<ClassTypeService>();
builder.Services.AddScoped<ClassScheduleService>();
builder.Services.AddScoped<BookingService>();

// Controllers with JSON enum-as-string serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global error handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenith Fitness Studio API v1");
});

app.UseHttpsRedirection();
app.MapControllers();

// Ensure DB is created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

app.Run();
