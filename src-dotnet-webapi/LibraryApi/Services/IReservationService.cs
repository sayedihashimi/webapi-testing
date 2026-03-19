using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct);
    Task<ReservationResponse> CancelAsync(int id, CancellationToken ct);
    Task<LoanResponse> FulfillAsync(int id, CancellationToken ct);
}
