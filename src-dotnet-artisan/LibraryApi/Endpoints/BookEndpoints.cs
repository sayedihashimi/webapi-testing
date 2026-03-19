using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this RouteGroupBuilder group)
    {
        var books = group.MapGroup("/books").WithTags("Books");

        books.MapGet("/", GetBooksAsync)
            .WithSummary("List books with search, filter, sorting, and pagination");

        books.MapGet("/{id:int}", GetBookByIdAsync)
            .WithSummary("Get book details including authors, categories, and availability");

        books.MapPost("/", CreateBookAsync)
            .WithSummary("Create a new book with author and category IDs");

        books.MapPut("/{id:int}", UpdateBookAsync)
            .WithSummary("Update an existing book");

        books.MapDelete("/{id:int}", DeleteBookAsync)
            .WithSummary("Delete a book (fails if book has active loans)");

        books.MapGet("/{id:int}/loans", GetBookLoansAsync)
            .WithSummary("Get loan history for a specific book");

        books.MapGet("/{id:int}/reservations", GetBookReservationsAsync)
            .WithSummary("Get active reservations queue for a specific book");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<BookResponse>>> GetBooksAsync(
        IBookService service, string? search, string? category, bool? available, string? sortBy,
        int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetBooksAsync(search, category, available, sortBy, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BookDetailResponse>, NotFound>> GetBookByIdAsync(
        int id, IBookService service, CancellationToken ct = default)
    {
        var result = await service.GetBookByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Created<BookDetailResponse>> CreateBookAsync(
        CreateBookRequest request, IBookService service, CancellationToken ct = default)
    {
        var result = await service.CreateBookAsync(request, ct);
        return TypedResults.Created($"/api/books/{result.Id}", result);
    }

    private static async Task<Results<Ok<BookDetailResponse>, NotFound>> UpdateBookAsync(
        int id, UpdateBookRequest request, IBookService service, CancellationToken ct = default)
    {
        var result = await service.UpdateBookAsync(id, request, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteBookAsync(
        int id, IBookService service, CancellationToken ct = default)
    {
        var (found, hasActiveLoans) = await service.DeleteBookAsync(id, ct);
        if (!found)
        {
            return TypedResults.NotFound();
        }

        return hasActiveLoans
            ? TypedResults.Conflict("Cannot delete book because it has active loans.")
            : TypedResults.NoContent();
    }

    private static async Task<Ok<PaginatedResponse<LoanResponse>>> GetBookLoansAsync(
        int id, IBookService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetBookLoansAsync(id, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PaginatedResponse<ReservationResponse>>> GetBookReservationsAsync(
        int id, IBookService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetBookReservationsAsync(id, page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}
