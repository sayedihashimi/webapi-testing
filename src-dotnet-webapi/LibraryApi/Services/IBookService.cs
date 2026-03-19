using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetAllAsync(string? search, int? categoryId, int? authorId, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken ct);
    Task<BookDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken ct);
    Task<BookResponse?> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(int bookId, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<ReservationResponse>> GetReservationsAsync(int bookId, CancellationToken ct);
}
