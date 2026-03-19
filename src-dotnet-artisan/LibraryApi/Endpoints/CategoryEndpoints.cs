using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
    {
        var categories = group.MapGroup("/categories").WithTags("Categories");

        categories.MapGet("/", GetCategoriesAsync)
            .WithSummary("List all categories with pagination");

        categories.MapGet("/{id:int}", GetCategoryByIdAsync)
            .WithSummary("Get category details with book count");

        categories.MapPost("/", CreateCategoryAsync)
            .WithSummary("Create a new category");

        categories.MapPut("/{id:int}", UpdateCategoryAsync)
            .WithSummary("Update an existing category");

        categories.MapDelete("/{id:int}", DeleteCategoryAsync)
            .WithSummary("Delete a category (fails if category has books)");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<CategoryResponse>>> GetCategoriesAsync(
        ICategoryService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetCategoriesAsync(page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryDetailResponse>, NotFound>> GetCategoryByIdAsync(
        int id, ICategoryService service, CancellationToken ct = default)
    {
        var result = await service.GetCategoryByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Created<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request, ICategoryService service, CancellationToken ct = default)
    {
        var result = await service.CreateCategoryAsync(request, ct);
        return TypedResults.Created($"/api/categories/{result.Id}", result);
    }

    private static async Task<Results<Ok<CategoryResponse>, NotFound>> UpdateCategoryAsync(
        int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct = default)
    {
        var result = await service.UpdateCategoryAsync(id, request, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteCategoryAsync(
        int id, ICategoryService service, CancellationToken ct = default)
    {
        var (found, hasBooks) = await service.DeleteCategoryAsync(id, ct);
        if (!found)
        {
            return TypedResults.NotFound();
        }

        return hasBooks
            ? TypedResults.Conflict("Cannot delete category because it has associated books.")
            : TypedResults.NoContent();
    }
}
