using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct);
    Task<PatronDetailResponse?> GetPatronByIdAsync(int id, CancellationToken ct);
    Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request, CancellationToken ct);
    Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request, CancellationToken ct);
    Task<(bool Found, bool HasActiveLoans)> DeactivatePatronAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct);
}
