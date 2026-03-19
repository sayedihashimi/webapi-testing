using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this RouteGroupBuilder group)
    {
        var authors = group.MapGroup("/authors").WithTags("Authors");

        authors.MapGet("/", GetAuthorsAsync)
            .WithSummary("List authors with optional name search and pagination");

        authors.MapGet("/{id:int}", GetAuthorByIdAsync)
            .WithSummary("Get author details including their books");

        authors.MapPost("/", CreateAuthorAsync)
            .WithSummary("Create a new author");

        authors.MapPut("/{id:int}", UpdateAuthorAsync)
            .WithSummary("Update an existing author");

        authors.MapDelete("/{id:int}", DeleteAuthorAsync)
            .WithSummary("Delete an author (fails if the author has books)");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<AuthorResponse>>> GetAuthorsAsync(
        IAuthorService service, string? search, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetAuthorsAsync(search, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<AuthorDetailResponse>, NotFound>> GetAuthorByIdAsync(
        int id, IAuthorService service, CancellationToken ct = default)
    {
        var result = await service.GetAuthorByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Created<AuthorResponse>> CreateAuthorAsync(
        CreateAuthorRequest request, IAuthorService service, CancellationToken ct = default)
    {
        var result = await service.CreateAuthorAsync(request, ct);
        return TypedResults.Created($"/api/authors/{result.Id}", result);
    }

    private static async Task<Results<Ok<AuthorResponse>, NotFound>> UpdateAuthorAsync(
        int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct = default)
    {
        var result = await service.UpdateAuthorAsync(id, request, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteAuthorAsync(
        int id, IAuthorService service, CancellationToken ct = default)
    {
        var (found, hasBooks) = await service.DeleteAuthorAsync(id, ct);
        if (!found)
        {
            return TypedResults.NotFound();
        }

        return hasBooks
            ? TypedResults.Conflict("Cannot delete author because they have associated books.")
            : TypedResults.NoContent();
    }
}
