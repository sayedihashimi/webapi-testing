using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async Task<Ok<PaginatedResponse<AuthorResponse>>> (
            string? search, int? page, int? pageSize,
            IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("Returns a paginated list of authors. Optionally filter by name.");

        group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author by ID")
        .WithDescription("Returns author details including their books.");

        group.MapPost("/", async Task<Created<AuthorResponse>> (
            CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/authors/{result.Id}", result);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create a new author");

        group.MapPut("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound>> (
            int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author");

        group.MapDelete("/{id:int}", async Task<NoContent> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteAuthor")
        .WithSummary("Delete an author")
        .WithDescription("Fails if the author has associated books.");

        return group;
    }
}
