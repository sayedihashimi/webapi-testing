using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetBooks).WithName("GetBooks");
        group.MapGet("/{id:int}", GetBook).WithName("GetBook");
        group.MapPost("/", CreateBook).WithName("CreateBook");
        group.MapPut("/{id:int}", UpdateBook).WithName("UpdateBook");
        group.MapDelete("/{id:int}", DeleteBook).WithName("DeleteBook");
        group.MapGet("/{id:int}/loans", GetBookLoans).WithName("GetBookLoans");
        group.MapGet("/{id:int}/reservations", GetBookReservations).WithName("GetBookReservations");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<BookSummaryResponse>>> GetBooks(
        LibraryService service,
        string? search = null, string? category = null, bool? available = null,
        string? sortBy = null, string? sortOrder = null,
        int page = 1, int pageSize = 10)
    {
        var result = await service.GetBooksAsync(search, category, available, sortBy, sortOrder, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BookDetailResponse>, NotFound>> GetBook(
        LibraryService service, int id)
    {
        var result = await service.GetBookByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<BookDetailResponse>, Conflict<string>>> CreateBook(
        LibraryService service, CreateBookRequest request)
    {
        var (result, error) = await service.CreateBookAsync(request);
        return result is not null
            ? TypedResults.Created($"/api/books/{result.Id}", result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<Ok<BookDetailResponse>, NotFound, Conflict<string>>> UpdateBook(
        LibraryService service, int id, UpdateBookRequest request)
    {
        var (result, error) = await service.UpdateBookAsync(id, request);
        if (error == "Book not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.Conflict(error!);
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteBook(
        LibraryService service, int id)
    {
        var (success, error) = await service.DeleteBookAsync(id);
        if (error == "Book not found") { return TypedResults.NotFound(); }
        if (!success) { return TypedResults.Conflict(error!); }
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<LoanResponse>>> GetBookLoans(LibraryService service, int id)
    {
        var result = await service.GetBookLoansAsync(id);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<ReservationResponse>>> GetBookReservations(LibraryService service, int id)
    {
        var result = await service.GetBookReservationsAsync(id);
        return TypedResults.Ok(result);
    }
}
