using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanDto>> GetAllAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<(LoanDto? Loan, string? Error)> CheckoutAsync(CreateLoanDto dto);
    Task<(LoanDto? Loan, string? Error)> ReturnAsync(int id);
    Task<(LoanDto? Loan, string? Error)> RenewAsync(int id);
    Task<PaginatedResponse<LoanDto>> GetOverdueAsync(int page, int pageSize);
}
