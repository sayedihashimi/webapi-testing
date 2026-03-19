using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, int page, int pageSize);
    Task<BookDetailDto> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(BookCreateDto dto);
    Task<BookDto> UpdateBookAsync(int id, BookUpdateDto dto);
    Task DeleteBookAsync(int id);
    Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, int page, int pageSize);
}
