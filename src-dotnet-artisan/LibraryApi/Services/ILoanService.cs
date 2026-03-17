using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetAllAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<LoanDto> CheckoutAsync(LoanCreateDto dto);
    Task<LoanDto> ReturnAsync(int id);
    Task<LoanDto> RenewAsync(int id);
    Task<List<LoanDto>> GetOverdueAsync();
}
