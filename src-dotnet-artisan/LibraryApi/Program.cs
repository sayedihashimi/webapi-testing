using System.Text.Json.Serialization;
using LibraryApi.Data;
using LibraryApi.Endpoints;
using LibraryApi.Middleware;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=library.db"));

// JSON enum serialization
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Services
builder.Services.AddScoped<LibraryService>();

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapAuthorEndpoints();
app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapPatronEndpoints();
app.MapLoanEndpoints();
app.MapReservationEndpoints();
app.MapFineEndpoints();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db);
}

app.Run();
