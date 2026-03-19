using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Patron;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models.Enums;

namespace LibraryApi.Services.Interfaces;

public interface IPatronService
{
    Task<PagedResult<PatronListDto>> GetAllAsync(string? search, MembershipType? membershipType, bool? isActive, PaginationParams pagination);
    Task<PatronDetailDto> GetByIdAsync(int id);
    Task<PatronDetailDto> CreateAsync(CreatePatronDto dto);
    Task<PatronDetailDto> UpdateAsync(int id, UpdatePatronDto dto);
    Task DeleteAsync(int id);
    Task<PagedResult<LoanListDto>> GetPatronLoansAsync(int patronId, LoanStatus? status, PaginationParams pagination);
    Task<List<ReservationListDto>> GetPatronReservationsAsync(int patronId);
    Task<PagedResult<FineListDto>> GetPatronFinesAsync(int patronId, FineStatus? status, PaginationParams pagination);
}
