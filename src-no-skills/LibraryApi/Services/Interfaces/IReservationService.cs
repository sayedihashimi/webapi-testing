using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models.Enums;

namespace LibraryApi.Services.Interfaces;

public interface IReservationService
{
    Task<PagedResult<ReservationListDto>> GetAllAsync(ReservationStatus? status, PaginationParams pagination);
    Task<ReservationDetailDto> GetByIdAsync(int id);
    Task<ReservationDetailDto> CreateAsync(CreateReservationDto dto);
    Task CancelAsync(int id);
    Task<ReservationDetailDto> FulfillAsync(int id);
}
