using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken cancellationToken);
    Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken cancellationToken);
    Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken cancellationToken);
    Task<PagedResult<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken cancellationToken);
}
