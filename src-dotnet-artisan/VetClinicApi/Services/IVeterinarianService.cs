using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct = default);
    Task<VeterinarianDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto, CancellationToken ct = default);
    Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct = default);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
