using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PaginatedResponse<BookDto>> GetAllAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookDto dto);
    Task<BookDto?> UpdateAsync(int id, UpdateBookDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<PaginatedResponse<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PaginatedResponse<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}
