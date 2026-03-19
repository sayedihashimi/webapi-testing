using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, int page, int pageSize);
    Task<ReservationDto> GetReservationByIdAsync(int id);
    Task<ReservationDto> CreateReservationAsync(ReservationCreateDto dto);
    Task<ReservationDto> CancelReservationAsync(int id);
    Task<LoanDto> FulfillReservationAsync(int id);
}
