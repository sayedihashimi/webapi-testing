using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize);
    Task<PatronDetailDto> GetPatronByIdAsync(int id);
    Task<PatronDto> CreatePatronAsync(PatronCreateDto dto);
    Task<PatronDto> UpdatePatronAsync(int id, PatronUpdateDto dto);
    Task DeactivatePatronAsync(int id);
    Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize);
    Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize);
    Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize);
}
