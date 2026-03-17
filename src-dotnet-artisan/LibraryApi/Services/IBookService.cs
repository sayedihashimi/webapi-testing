using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetAllAsync(string? search, bool? available, string? sortBy, string? sortDir, int page, int pageSize);
    Task<BookDetailDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(BookCreateDto dto);
    Task<BookDto?> UpdateAsync(int id, BookUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<LoanDto>> GetLoansAsync(int bookId, int page, int pageSize);
    Task<List<ReservationDto>> GetReservationsAsync(int bookId);
}
