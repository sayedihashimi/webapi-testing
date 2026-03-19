using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDto> GetLoanByIdAsync(int id);
    Task<LoanDto> CheckoutBookAsync(LoanCreateDto dto);
    Task<LoanDto> ReturnBookAsync(int id);
    Task<LoanDto> RenewLoanAsync(int id);
    Task<PagedResult<LoanDto>> GetOverdueLoansAsync(int page, int pageSize);
}
