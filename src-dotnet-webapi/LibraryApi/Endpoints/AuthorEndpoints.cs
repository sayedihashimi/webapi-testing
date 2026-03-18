using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (string? search, int? page, int? pageSize, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("Returns a paginated list of authors. Supports searching by name.")
        .Produces<PagedResponse<AuthorResponse>>();

        group.MapGet("/{id:int}", async (int id, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author by ID")
        .WithDescription("Returns author details including their books.")
        .Produces<AuthorDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/authors/{result.Id}", result);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create a new author")
        .WithDescription("Creates a new author and returns the created resource.")
        .Produces<AuthorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author")
        .WithDescription("Updates an existing author's information.")
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
