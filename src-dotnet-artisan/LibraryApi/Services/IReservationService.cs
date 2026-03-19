using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationDetailResponse?> GetReservationByIdAsync(int id, CancellationToken ct);
    Task<Result<ReservationDetailResponse>> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct);
    Task<Result<ReservationDetailResponse>> CancelReservationAsync(int id, CancellationToken ct);
    Task<Result<LoanDetailResponse>> FulfillReservationAsync(int id, CancellationToken ct);
}
