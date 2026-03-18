using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IAuthorService
{
    Task<PagedResult<AuthorDto>> GetAuthorsAsync(string? search, PaginationParams pagination);
    Task<AuthorDetailDto> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto dto);
    Task<AuthorDto> UpdateAuthorAsync(int id, UpdateAuthorDto dto);
    Task DeleteAuthorAsync(int id);
}

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(PaginationParams pagination);
    Task<CategoryDetailDto> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int id);
}

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(string? search, string? category, bool? available, string? sortBy, string? sortOrder, PaginationParams pagination);
    Task<BookDto> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(CreateBookDto dto);
    Task<BookDto> UpdateBookAsync(int id, UpdateBookDto dto);
    Task DeleteBookAsync(int id);
    Task<PagedResult<LoanDto>> GetBookLoansAsync(int bookId, PaginationParams pagination);
    Task<PagedResult<ReservationDto>> GetBookReservationsAsync(int bookId, PaginationParams pagination);
}

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, PaginationParams pagination);
    Task<PatronDetailDto> GetPatronByIdAsync(int id);
    Task<PatronDto> CreatePatronAsync(CreatePatronDto dto);
    Task<PatronDto> UpdatePatronAsync(int id, UpdatePatronDto dto);
    Task DeactivatePatronAsync(int id);
    Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, PaginationParams pagination);
    Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, PaginationParams pagination);
    Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, PaginationParams pagination);
}

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, PaginationParams pagination);
    Task<LoanDto> GetLoanByIdAsync(int id);
    Task<LoanDto> CheckoutBookAsync(CreateLoanDto dto);
    Task<LoanDto> ReturnBookAsync(int id);
    Task<LoanDto> RenewLoanAsync(int id);
    Task<PagedResult<LoanDto>> GetOverdueLoansAsync(PaginationParams pagination);
}

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, PaginationParams pagination);
    Task<ReservationDto> GetReservationByIdAsync(int id);
    Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto);
    Task<ReservationDto> CancelReservationAsync(int id);
    Task<LoanDto> FulfillReservationAsync(int id);
}

public interface IFineService
{
    Task<PagedResult<FineDto>> GetFinesAsync(string? status, PaginationParams pagination);
    Task<FineDto> GetFineByIdAsync(int id);
    Task<FineDto> PayFineAsync(int id);
    Task<FineDto> WaiveFineAsync(int id);
}
