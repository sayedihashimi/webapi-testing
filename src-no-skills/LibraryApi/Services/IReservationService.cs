using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationDto>> GetAllAsync(string? status, int page, int pageSize);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<(ReservationDto? Reservation, string? Error)> CreateAsync(CreateReservationDto dto);
    Task<(ReservationDto? Reservation, string? Error)> CancelAsync(int id);
    Task<(LoanDto? Loan, string? Error)> FulfillAsync(int id);
}
