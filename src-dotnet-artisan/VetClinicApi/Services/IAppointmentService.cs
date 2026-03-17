using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination);
    Task<AppointmentDetailDto?> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<IReadOnlyList<AppointmentDto>> GetTodayAsync();
}
