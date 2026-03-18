using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (string? search, string? category, bool? available, string? sortBy, int? page, int? pageSize, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, category, available, sortBy, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("Returns a paginated list of books. Supports searching by title/author/ISBN, filtering by category and availability, and sorting.")
        .Produces<PagedResponse<BookResponse>>();

        group.MapGet("/{id:int}", async (int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetBookById")
        .WithSummary("Get book by ID")
        .WithDescription("Returns book details including authors, categories, and availability.")
        .Produces<BookResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/books/{result.Id}", result);
        })
        .WithName("CreateBook")
        .WithSummary("Create a new book")
        .WithDescription("Creates a new book with author and category associations.")
        .Produces<BookResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book")
        .WithDescription("Updates an existing book's information and associations.")
        .Produces<BookResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, IBookService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteBook")
        .WithSummary("Delete a book")
        .WithDescription("Deletes a book. Fails if the book has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async (int id, int? page, int? pageSize, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetLoansAsync(id, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetBookLoans")
        .WithSummary("Get book loan history")
        .WithDescription("Returns the loan history for a specific book.")
        .Produces<PagedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async (int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("GetBookReservations")
        .WithSummary("Get active book reservations")
        .WithDescription("Returns active reservations for a specific book, ordered by queue position.")
        .Produces<List<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
