using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/books")
            .WithTags("Books");

        group.MapGet("/", GetBooksAsync);
        group.MapGet("/{id:int}", GetBookByIdAsync);
        group.MapPost("/", CreateBookAsync);
        group.MapPut("/{id:int}", UpdateBookAsync);
        group.MapDelete("/{id:int}", DeleteBookAsync);
        group.MapGet("/{id:int}/loans", GetBookLoansAsync);
        group.MapGet("/{id:int}/reservations", GetBookReservationsAsync);

        return group;
    }

    private static async Task<IResult> GetBooksAsync(
        IBookService service,
        string? search = null,
        string? category = null,
        bool? available = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetBooksAsync(search, category, available, sortBy, sortDirection, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetBookByIdAsync(
        int id,
        IBookService service,
        CancellationToken ct = default)
    {
        var book = await service.GetBookByIdAsync(id, ct);
        return book is not null
            ? TypedResults.Ok(book)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateBookAsync(
        CreateBookDto dto,
        IBookService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.ISBN))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["Title and ISBN are required."] });
        }

        if (dto.TotalCopies < 1)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["TotalCopies"] = ["TotalCopies must be at least 1."] });
        }

        var book = await service.CreateBookAsync(dto, ct);
        return TypedResults.Created($"/api/books/{book.Id}", book);
    }

    private static async Task<IResult> UpdateBookAsync(
        int id,
        UpdateBookDto dto,
        IBookService service,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.ISBN))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [""] = ["Title and ISBN are required."] });
        }

        var book = await service.UpdateBookAsync(id, dto, ct);
        return book is not null
            ? TypedResults.Ok(book)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteBookAsync(
        int id,
        IBookService service,
        CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await service.DeleteBookAsync(id, ct);

        if (!found)
        {
            return TypedResults.NotFound();
        }

        if (hasActiveLoans)
        {
            return TypedResults.Problem(
                detail: "Cannot delete book with active loans.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetBookLoansAsync(
        int id,
        IBookService service,
        CancellationToken ct = default)
    {
        var loans = await service.GetBookLoansAsync(id, ct);
        return TypedResults.Ok(loans);
    }

    private static async Task<IResult> GetBookReservationsAsync(
        int id,
        IBookService service,
        CancellationToken ct = default)
    {
        var reservations = await service.GetBookReservationsAsync(id, ct);
        return TypedResults.Ok(reservations);
    }
}
