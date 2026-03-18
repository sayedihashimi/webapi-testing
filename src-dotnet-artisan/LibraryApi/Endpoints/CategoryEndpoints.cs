using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", GetCategories).WithName("GetCategories");
        group.MapGet("/{id:int}", GetCategory).WithName("GetCategory");
        group.MapPost("/", CreateCategory).WithName("CreateCategory");
        group.MapPut("/{id:int}", UpdateCategory).WithName("UpdateCategory");
        group.MapDelete("/{id:int}", DeleteCategory).WithName("DeleteCategory");

        return group;
    }

    private static async Task<Ok<List<CategoryResponse>>> GetCategories(LibraryService service)
    {
        var result = await service.GetCategoriesAsync();
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CategoryDetailResponse>, NotFound>> GetCategory(
        LibraryService service, int id)
    {
        var result = await service.GetCategoryByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<CategoryResponse>, Conflict<string>>> CreateCategory(
        LibraryService service, CreateCategoryRequest request)
    {
        var (result, error) = await service.CreateCategoryAsync(request);
        return result is not null
            ? TypedResults.Created($"/api/categories/{result.Id}", result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<Ok<CategoryResponse>, NotFound, Conflict<string>>> UpdateCategory(
        LibraryService service, int id, UpdateCategoryRequest request)
    {
        var (result, error) = await service.UpdateCategoryAsync(id, request);
        if (error == "Category not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteCategory(
        LibraryService service, int id)
    {
        var (success, error) = await service.DeleteCategoryAsync(id);
        if (error == "Category not found") { return TypedResults.NotFound(); }
        if (!success) { return TypedResults.Conflict(error!); }
        return TypedResults.NoContent();
    }
}
