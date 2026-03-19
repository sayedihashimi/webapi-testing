using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async Task<Ok<PaginatedResponse<BookResponse>>> (
            string? search, int? categoryId, int? authorId,
            string? sortBy, string? sortDirection,
            int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, categoryId, authorId, sortBy, sortDirection, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("Returns a paginated list of books with optional search, filtering by category/author, and sorting.")
        .Produces<PaginatedResponse<BookResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<BookDetailResponse>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetBookById")
        .WithSummary("Get book by ID")
        .WithDescription("Returns book details including authors, categories, and availability.")
        .Produces<BookDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<BookResponse>> (
            CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/books/{result.Id}", result);
        })
        .WithName("CreateBook")
        .WithSummary("Create a book")
        .WithDescription("Creates a new book with associated authors and categories.")
        .Produces<BookResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<BookResponse>, NotFound>> (
            int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book")
        .WithDescription("Updates an existing book by ID.")
        .Produces<BookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteBook")
        .WithSummary("Delete a book")
        .WithDescription("Deletes a book. Fails if the book has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            int id, int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetLoansAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookLoans")
        .WithSummary("Get book loan history")
        .WithDescription("Returns paginated loan history for a specific book.")
        .Produces<PaginatedResponse<LoanResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Ok<IReadOnlyList<ReservationResponse>>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookReservations")
        .WithSummary("Get active book reservations")
        .WithDescription("Returns active reservations for a specific book.")
        .Produces<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
