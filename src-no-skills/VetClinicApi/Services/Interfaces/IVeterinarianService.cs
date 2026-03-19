using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Veterinarian;
using VetClinicApi.DTOs.Appointment;

namespace VetClinicApi.Services.Interfaces;

public interface IVeterinarianService
{
    Task<PagedResult<VeterinarianDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination);
    Task<VeterinarianDto> GetByIdAsync(int id);
    Task<VeterinarianDto> CreateAsync(CreateVeterinarianDto dto);
    Task<VeterinarianDto> UpdateAsync(int id, UpdateVeterinarianDto dto);
    Task<List<AppointmentDto>> GetScheduleAsync(int vetId, DateOnly date);
    Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int vetId, PaginationParams pagination);
}
