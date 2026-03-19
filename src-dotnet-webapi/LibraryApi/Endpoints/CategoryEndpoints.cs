using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async Task<Ok<IReadOnlyList<CategoryResponse>>> (
            ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("List all categories");

        group.MapGet("/{id:int}", async Task<Results<Ok<CategoryDetailResponse>, NotFound>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Returns category details with book count.");

        group.MapPost("/", async Task<Created<CategoryResponse>> (
            CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category");

        group.MapPut("/{id:int}", async Task<Results<Ok<CategoryResponse>, NotFound>> (
            int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category");

        group.MapDelete("/{id:int}", async Task<NoContent> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .WithDescription("Fails if the category has associated books.");

        return group;
    }
}
