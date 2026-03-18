using LibraryApi.DTOs;
using LibraryApi.Models;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PagedResponse<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct);
    Task<ReservationResponse> CancelAsync(int id, CancellationToken ct);
    Task<ReservationResponse> FulfillAsync(int id, CancellationToken ct);
}
