using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetAllAsync(ReservationStatus? status, int page, int pageSize);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto> CreateAsync(ReservationCreateDto dto);
    Task<ReservationDto> CancelAsync(int id);
    Task<LoanDto> FulfillAsync(int id);
}
