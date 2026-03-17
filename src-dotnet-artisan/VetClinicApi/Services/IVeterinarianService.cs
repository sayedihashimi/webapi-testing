using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination);
    Task<VeterinarianDto?> GetByIdAsync(int id);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianDto?> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<IReadOnlyList<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination);
}
