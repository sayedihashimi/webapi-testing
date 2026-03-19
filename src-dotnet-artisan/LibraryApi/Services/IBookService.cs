using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, int page, int pageSize, CancellationToken ct);
    Task<BookDetailResponse?> GetBookByIdAsync(int id, CancellationToken ct);
    Task<BookDetailResponse> CreateBookAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookDetailResponse?> UpdateBookAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetBookLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetBookReservationsAsync(int bookId, int page, int pageSize, CancellationToken ct);
}
