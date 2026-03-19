using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/authors")
            .WithTags("Authors");

        group.MapGet("/", GetAuthorsAsync);
        group.MapGet("/{id:int}", GetAuthorByIdAsync);
        group.MapPost("/", CreateAuthorAsync);
        group.MapPut("/{id:int}", UpdateAuthorAsync);
        group.MapDelete("/{id:int}", DeleteAuthorAsync);

        return group;
    }

    private static async Task<IResult> GetAuthorsAsync(
        IAuthorService service,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAuthorsAsync(search, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetAuthorByIdAsync(
        int id,
        IAuthorService service,
        CancellationToken ct = default)
    {
        var author = await service.GetAuthorByIdAsync(id, ct);
        return author is not null
            ? TypedResults.Ok(author)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAuthorAsync(
        CreateAuthorDto dto,
        IAuthorService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["FirstName and LastName are required."] });
        }

        var author = await service.CreateAuthorAsync(dto, ct);
        return TypedResults.Created($"/api/authors/{author.Id}", author);
    }

    private static async Task<IResult> UpdateAuthorAsync(
        int id,
        UpdateAuthorDto dto,
        IAuthorService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["FirstName and LastName are required."] });
        }

        var author = await service.UpdateAuthorAsync(id, dto, ct);
        return author is not null
            ? TypedResults.Ok(author)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteAuthorAsync(
        int id,
        IAuthorService service,
        CancellationToken ct = default)
    {
        var (found, hasBooks) = await service.DeleteAuthorAsync(id, ct);

        if (!found)
        {
            return TypedResults.NotFound();
        }

        if (hasBooks)
        {
            return TypedResults.Problem(
                detail: "Cannot delete author with associated books.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return TypedResults.NoContent();
    }
}
