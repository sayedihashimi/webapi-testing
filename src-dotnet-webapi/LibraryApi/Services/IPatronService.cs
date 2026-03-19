using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct);
    Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct);
    Task<PatronResponse> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(int patronId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<FineResponse>> GetFinesAsync(int patronId, FineStatus? status, int page, int pageSize, CancellationToken ct);
}
