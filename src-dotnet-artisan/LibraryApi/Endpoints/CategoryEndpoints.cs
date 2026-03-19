using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapGet("/", GetCategoriesAsync);
        group.MapGet("/{id:int}", GetCategoryByIdAsync);
        group.MapPost("/", CreateCategoryAsync);
        group.MapPut("/{id:int}", UpdateCategoryAsync);
        group.MapDelete("/{id:int}", DeleteCategoryAsync);

        return group;
    }

    private static async Task<IResult> GetCategoriesAsync(
        ICategoryService service,
        CancellationToken ct = default)
    {
        var result = await service.GetCategoriesAsync(ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetCategoryByIdAsync(
        int id,
        ICategoryService service,
        CancellationToken ct = default)
    {
        var category = await service.GetCategoryByIdAsync(id, ct);
        return category is not null
            ? TypedResults.Ok(category)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateCategoryAsync(
        CreateCategoryDto dto,
        ICategoryService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
        }

        var category = await service.CreateCategoryAsync(dto, ct);
        return TypedResults.Created($"/api/categories/{category.Id}", category);
    }

    private static async Task<IResult> UpdateCategoryAsync(
        int id,
        UpdateCategoryDto dto,
        ICategoryService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
        }

        var category = await service.UpdateCategoryAsync(id, dto, ct);
        return category is not null
            ? TypedResults.Ok(category)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteCategoryAsync(
        int id,
        ICategoryService service,
        CancellationToken ct = default)
    {
        var (found, hasBooks) = await service.DeleteCategoryAsync(id, ct);

        if (!found)
        {
            return TypedResults.NotFound();
        }

        if (hasBooks)
        {
            return TypedResults.Problem(
                detail: "Cannot delete category with associated books.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return TypedResults.NoContent();
    }
}
