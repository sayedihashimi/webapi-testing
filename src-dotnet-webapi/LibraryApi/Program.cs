using System.Text.Json.Serialization;
using LibraryApi;
using LibraryApi.Data;
using LibraryApi.Endpoints;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JSON enum serialization as strings
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI + Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Sunrise Community Library API", Version = "v1" });
});

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

var app = builder.Build();

// Middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API v1"));
}

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
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

app.Run();
