using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Appointment;
using VetClinicApi.Models.Enums;

namespace VetClinicApi.Services.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetAllAsync(DateOnly? date, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination);
    Task<AppointmentDetailDto> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<List<AppointmentDto>> GetTodayAsync();
}
