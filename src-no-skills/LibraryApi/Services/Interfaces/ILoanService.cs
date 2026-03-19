using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.Models.Enums;

namespace LibraryApi.Services.Interfaces;

public interface ILoanService
{
    Task<PagedResult<LoanListDto>> GetAllAsync(LoanStatus? status, PaginationParams pagination);
    Task<LoanDetailDto> GetByIdAsync(int id);
    Task<LoanDetailDto> CheckoutAsync(CreateLoanDto dto);
    Task<ReturnLoanDto> ReturnAsync(int id);
    Task<RenewLoanDto> RenewAsync(int id);
    Task<List<LoanListDto>> GetOverdueLoansAsync();
}
