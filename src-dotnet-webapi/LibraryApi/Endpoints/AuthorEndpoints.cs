using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (string? search, int? page, int? pageSize, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("Returns a paginated list of authors. Optionally filter by name.")
        .Produces<PaginatedResponse<AuthorResponse>>();

        group.MapGet("/{id:int}", async (int id, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.GetByIdAsync(id, ct);
            return author is null ? Results.NotFound() : Results.Ok(author);
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author by ID")
        .WithDescription("Returns author details including their books.")
        .Produces<AuthorDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.CreateAsync(request, ct);
            return Results.Created($"/api/authors/{author.Id}", author);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create an author")
        .WithDescription("Creates a new author record.")
        .Produces<AuthorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.UpdateAsync(id, request, ct);
            return author is null ? Results.NotFound() : Results.Ok(author);
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author")
        .WithDescription("Updates an existing author record.")
        .Produces<AuthorResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, IAuthorService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteAuthor")
        .WithSummary("Delete an author")
        .WithDescription("Deletes an author. Fails if the author has associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
