using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronDto>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize);
    Task<PatronDetailDto?> GetByIdAsync(int id);
    Task<PatronDto> CreateAsync(CreatePatronDto dto);
    Task<PatronDto?> UpdateAsync(int id, UpdatePatronDto dto);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<PaginatedResponse<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PaginatedResponse<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PaginatedResponse<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}
