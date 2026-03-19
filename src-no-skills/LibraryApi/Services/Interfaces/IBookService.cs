using LibraryApi.DTOs.Book;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Reservation;

namespace LibraryApi.Services.Interfaces;

public interface IBookService
{
    Task<PagedResult<BookListDto>> GetAllAsync(string? search, int? categoryId, int? authorId, string? sortBy, string? sortOrder, PaginationParams pagination);
    Task<BookDetailDto> GetByIdAsync(int id);
    Task<BookDetailDto> CreateAsync(CreateBookDto dto);
    Task<BookDetailDto> UpdateAsync(int id, UpdateBookDto dto);
    Task DeleteAsync(int id);
    Task<PagedResult<LoanListDto>> GetBookLoansAsync(int bookId, PaginationParams pagination);
    Task<List<ReservationListDto>> GetBookReservationsAsync(int bookId);
}
