using VetClinicApi.DTOs;

namespace VetClinicApi.Services;

public interface IVeterinarianService
{
    Task<PagedResponse<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination);
    Task<VeterinarianResponseDto?> GetByIdAsync(int id);
    Task<VeterinarianResponseDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianResponseDto> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<List<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination);
}
