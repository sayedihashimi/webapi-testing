using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize, CancellationToken ct);
    Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookDetailResponse> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookDetailResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ReservationResponse>> GetReservationsAsync(int bookId, CancellationToken ct);
}
