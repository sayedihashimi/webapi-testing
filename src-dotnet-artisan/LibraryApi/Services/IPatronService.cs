using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize);
    Task<PatronDetailDto?> GetByIdAsync(int id);
    Task<PatronDto> CreateAsync(PatronCreateDto dto);
    Task<PatronDto?> UpdateAsync(int id, PatronUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<LoanDto>> GetLoansAsync(int patronId, LoanStatus? status, int page, int pageSize);
    Task<List<ReservationDto>> GetReservationsAsync(int patronId);
    Task<List<FineDto>> GetFinesAsync(int patronId, FineStatus? status);
}
