using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PagedResult<PatronSummaryDto>> GetPatronsAsync(string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct = default);
    Task<PatronDetailDto?> GetPatronByIdAsync(int id, CancellationToken ct = default);
    Task<PatronDetailDto> CreatePatronAsync(CreatePatronDto dto, CancellationToken ct = default);
    Task<PatronDetailDto?> UpdatePatronAsync(int id, UpdatePatronDto dto, CancellationToken ct = default);
    Task<(bool Found, bool HasActiveLoans)> DeactivatePatronAsync(int id, CancellationToken ct = default);
    Task<List<LoanDto>> GetPatronLoansAsync(int patronId, string? status, CancellationToken ct = default);
    Task<List<ReservationDto>> GetPatronReservationsAsync(int patronId, CancellationToken ct = default);
    Task<List<FineDto>> GetPatronFinesAsync(int patronId, string? status, CancellationToken ct = default);
}
