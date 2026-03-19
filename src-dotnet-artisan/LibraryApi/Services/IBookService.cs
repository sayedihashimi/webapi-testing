using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResult<BookSummaryDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken ct = default);
    Task<BookDetailDto?> GetBookByIdAsync(int id, CancellationToken ct = default);
    Task<BookDetailDto> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default);
    Task<BookDetailDto?> UpdateBookAsync(int id, UpdateBookDto dto, CancellationToken ct = default);
    Task<(bool Found, bool HasActiveLoans)> DeleteBookAsync(int id, CancellationToken ct = default);
    Task<List<LoanDto>> GetBookLoansAsync(int bookId, CancellationToken ct = default);
    Task<List<ReservationDto>> GetBookReservationsAsync(int bookId, CancellationToken ct = default);
}
