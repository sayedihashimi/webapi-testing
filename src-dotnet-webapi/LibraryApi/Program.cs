using System.Text.Json.Serialization;
using LibraryApi.Data;
using LibraryApi.Endpoints;
using LibraryApi.Middleware;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JSON enum serialization as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.UseAllOfToExtendReferenceSchemas());

// Error handling
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Services
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IPatronService, PatronService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IFineService, FineService>();
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Error handling middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

// OpenAPI & Swagger
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints
app.MapAuthorEndpoints();
app.MapCategoryEndpoints();
app.MapBookEndpoints();
app.MapPatronEndpoints();
app.MapLoanEndpoints();
app.MapReservationEndpoints();
app.MapFineEndpoints();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
