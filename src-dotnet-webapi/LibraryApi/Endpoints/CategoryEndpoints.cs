using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async Task<Ok<IReadOnlyList<CategoryResponse>>> (
            ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("List all categories")
        .WithDescription("Returns all categories.")
        .Produces<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<CategoryDetailResponse>, NotFound>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Returns category details with book count.")
        .Produces<CategoryDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<CategoryResponse>> (
            CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a category")
        .WithDescription("Creates a new category. Name must be unique.")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<CategoryResponse>, NotFound>> (
            int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category")
        .WithDescription("Updates an existing category by ID.")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .WithDescription("Deletes a category. Fails if the category has associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
