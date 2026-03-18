using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (int? page, int? pageSize, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("List categories")
        .WithDescription("Returns a paginated list of all categories.")
        .Produces<PaginatedResponse<CategoryResponse>>();

        group.MapGet("/{id:int}", async (int id, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.GetByIdAsync(id, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Returns category details including the count of associated books.")
        .Produces<CategoryDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.CreateAsync(request, ct);
            return Results.Created($"/api/categories/{category.Id}", category);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a category")
        .WithDescription("Creates a new book category. Name must be unique.")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.UpdateAsync(id, request, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category")
        .WithDescription("Updates an existing category. Name must remain unique.")
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
