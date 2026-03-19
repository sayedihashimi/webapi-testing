using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async Task<Ok<PaginatedResponse<BookResponse>>> (
            string? search, string? category, bool? available,
            string? sortBy, string? sortOrder,
            int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(
                search, category, available, sortBy, sortOrder,
                page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("Returns a paginated list of books. Filter by search term, category, or availability. Sort by title or year.");

        group.MapGet("/{id:int}", async Task<Results<Ok<BookDetailResponse>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetBookById")
        .WithSummary("Get book by ID")
        .WithDescription("Returns book details with authors, categories, and availability.");

        group.MapPost("/", async Task<Created<BookDetailResponse>> (
            CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/books/{result.Id}", result);
        })
        .WithName("CreateBook")
        .WithSummary("Create a new book");

        group.MapPut("/{id:int}", async Task<Results<Ok<BookDetailResponse>, NotFound>> (
            int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book");

        group.MapDelete("/{id:int}", async Task<NoContent> (
            int id, IBookService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteBook")
        .WithSummary("Delete a book")
        .WithDescription("Fails if the book has active loans.");

        group.MapGet("/{id:int}/loans", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            int id, int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetLoansAsync(id, page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookLoans")
        .WithSummary("Get loan history for a book");

        group.MapGet("/{id:int}/reservations", async Task<Ok<IReadOnlyList<ReservationResponse>>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetReservationsAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookReservations")
        .WithSummary("Get active reservation queue for a book");

        return group;
    }
}
