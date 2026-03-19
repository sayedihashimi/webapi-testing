using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<ReservationDto?> GetReservationByIdAsync(int id, CancellationToken ct = default);
    Task<(ReservationDto? Reservation, string? Error)> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default);
    Task<(ReservationDto? Reservation, string? Error)> CancelReservationAsync(int id, CancellationToken ct = default);
    Task<(LoanDto? Loan, string? Error)> FulfillReservationAsync(int id, CancellationToken ct = default);
}
