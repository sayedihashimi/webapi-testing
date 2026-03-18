using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResponse<AppointmentResponseDto>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination);
    Task<AppointmentResponseDto?> GetByIdAsync(int id);
    Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<List<AppointmentResponseDto>> GetTodayAsync();
}
