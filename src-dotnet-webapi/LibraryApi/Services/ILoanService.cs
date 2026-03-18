using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PagedResponse<LoanResponse>> GetAllAsync(LoanStatus? status, bool? overdue, int page, int pageSize, CancellationToken ct);
    Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct);
    Task<LoanResponse> ReturnAsync(int id, CancellationToken ct);
    Task<LoanResponse> RenewAsync(int id, CancellationToken ct);
    Task<List<LoanResponse>> GetOverdueAsync(CancellationToken ct);
}
