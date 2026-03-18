using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("List all categories")
        .WithDescription("Returns all book categories.")
        .Produces<List<CategoryResponse>>();

        group.MapGet("/{id:int}", async (int id, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Returns category details including the number of associated books.")
        .Produces<CategoryDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Creates a new book category. Category names must be unique.")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category")
        .WithDescription("Updates an existing category.")
        .Produces<CategoryResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .WithDescription("Deletes a category. Fails if the category has associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
