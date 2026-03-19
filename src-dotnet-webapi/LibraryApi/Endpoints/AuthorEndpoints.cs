using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async Task<Ok<PaginatedResponse<AuthorResponse>>> (
            string? search, int? page, int? pageSize,
            IAuthorService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("Returns a paginated list of authors with optional search by name.")
        .Produces<PaginatedResponse<AuthorResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author by ID")
        .WithDescription("Returns author details including associated books.")
        .Produces<AuthorDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<AuthorResponse>> (
            CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/authors/{result.Id}", result);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create an author")
        .WithDescription("Creates a new author record.")
        .Produces<AuthorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound>> (
            int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author")
        .WithDescription("Updates an existing author by ID.")
        .Produces<AuthorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteAuthor")
        .WithSummary("Delete an author")
        .WithDescription("Deletes an author. Fails if the author has associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
