using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using LibraryApi.Data;
using LibraryApi.Middleware;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers with JSON enum string serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure ProblemDetails
builder.Services.AddProblemDetails();

// Exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Services (interface + implementation)
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IPatronService, PatronService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IFineService, FineService>();

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sunrise Community Library API", Version = "v1", Description = "Community Library Management System API for Sunrise Community Library" });
});

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler();

// Map business rule exceptions to proper status codes
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (BusinessRuleException ex)
    {
        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = ex.StatusCode,
            Title = ex.StatusCode == 409 ? "Conflict" : "Bad Request",
            Detail = ex.Message,
            Instance = context.Request.Path
        });
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = ex.Message,
            Instance = context.Request.Path
        });
    }
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sunrise Community Library API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

app.Run();
