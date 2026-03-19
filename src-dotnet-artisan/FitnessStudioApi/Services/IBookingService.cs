using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BookingResponse> CancelAsync(int id, string? reason, CancellationToken ct = default);
    Task<BookingResponse> CheckInAsync(int id, CancellationToken ct = default);
    Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct = default);
}
