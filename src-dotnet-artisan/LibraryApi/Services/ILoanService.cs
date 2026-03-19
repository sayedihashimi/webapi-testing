using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default);
    Task<LoanDetailDto?> GetLoanByIdAsync(int id, CancellationToken ct = default);
    Task<(LoanDto? Loan, string? Error)> CheckoutBookAsync(CreateLoanDto dto, CancellationToken ct = default);
    Task<(LoanDto? Loan, string? Error)> ReturnBookAsync(int id, CancellationToken ct = default);
    Task<(LoanDto? Loan, string? Error)> RenewLoanAsync(int id, CancellationToken ct = default);
    Task<List<LoanDto>> GetOverdueLoansAsync(CancellationToken ct = default);
}
