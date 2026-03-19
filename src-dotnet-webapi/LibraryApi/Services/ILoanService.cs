using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetAllAsync(LoanStatus? status, int page, int pageSize, CancellationToken ct);
    Task<LoanDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct);
    Task<LoanResponse> ReturnAsync(int id, CancellationToken ct);
    Task<LoanResponse> RenewAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<LoanResponse>> GetOverdueAsync(CancellationToken ct);
}
