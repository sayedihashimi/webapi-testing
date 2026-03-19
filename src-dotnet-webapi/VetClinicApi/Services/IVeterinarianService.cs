using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PaginatedResponse<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct);
    Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct);
    Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct);
    Task<PaginatedResponse<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct);
}
