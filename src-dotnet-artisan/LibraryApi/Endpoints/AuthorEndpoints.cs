using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", GetAuthors).WithName("GetAuthors");
        group.MapGet("/{id:int}", GetAuthor).WithName("GetAuthor");
        group.MapPost("/", CreateAuthor).WithName("CreateAuthor");
        group.MapPut("/{id:int}", UpdateAuthor).WithName("UpdateAuthor");
        group.MapDelete("/{id:int}", DeleteAuthor).WithName("DeleteAuthor");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<AuthorResponse>>> GetAuthors(
        LibraryService service, string? search = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetAuthorsAsync(search, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<AuthorDetailResponse>, NotFound>> GetAuthor(
        LibraryService service, int id)
    {
        var result = await service.GetAuthorByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Created<AuthorResponse>> CreateAuthor(
        LibraryService service, CreateAuthorRequest request)
    {
        var result = await service.CreateAuthorAsync(request);
        return TypedResults.Created($"/api/authors/{result.Id}", result);
    }

    private static async Task<Results<Ok<AuthorResponse>, NotFound>> UpdateAuthor(
        LibraryService service, int id, UpdateAuthorRequest request)
    {
        var result = await service.UpdateAuthorAsync(id, request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteAuthor(
        LibraryService service, int id)
    {
        var (success, error) = await service.DeleteAuthorAsync(id);
        if (error == "Author not found") { return TypedResults.NotFound(); }
        if (!success) { return TypedResults.Conflict(error!); }
        return TypedResults.NoContent();
    }
}
