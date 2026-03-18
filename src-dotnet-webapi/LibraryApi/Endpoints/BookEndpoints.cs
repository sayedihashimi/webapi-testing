using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (string? search, int? categoryId, bool? available, string? sortBy, int? page, int? pageSize, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, categoryId, available, sortBy, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("Returns a paginated list of books. Filter by title/author/ISBN, category, availability. Sort by 'title' or 'year'.")
        .Produces<PaginatedResponse<BookResponse>>();

        group.MapGet("/{id:int}", async (int id, IBookService service, CancellationToken ct) =>
        {
            var book = await service.GetByIdAsync(id, ct);
            return book is null ? Results.NotFound() : Results.Ok(book);
        })
        .WithName("GetBookById")
        .WithSummary("Get book by ID")
        .WithDescription("Returns book details including authors, categories, and availability.")
        .Produces<BookResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var book = await service.CreateAsync(request, ct);
            return Results.Created($"/api/books/{book.Id}", book);
        })
        .WithName("CreateBook")
        .WithSummary("Create a book")
        .WithDescription("Creates a new book with author and category associations.")
        .Produces<BookResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var book = await service.UpdateAsync(id, request, ct);
            return book is null ? Results.NotFound() : Results.Ok(book);
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book")
        .WithDescription("Updates an existing book including author and category associations.")
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
            var result = await service.GetBookLoansAsync(id, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetBookLoans")
        .WithSummary("Get book loan history")
        .WithDescription("Returns the loan history for a specific book.")
        .Produces<PaginatedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async (int id, int? page, int? pageSize, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetBookReservationsAsync(id, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
            return Results.Ok(result);
        })
        .WithName("GetBookReservations")
        .WithSummary("Get active book reservations")
        .WithDescription("Returns the active reservation queue for a specific book.")
        .Produces<PaginatedResponse<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
