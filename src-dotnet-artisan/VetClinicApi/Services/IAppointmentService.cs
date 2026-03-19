using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct = default);
    Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto, CancellationToken ct = default);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct = default);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct = default);
    Task<bool> HasConflictAsync(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId = null, CancellationToken ct = default);
    Task<string?> ValidateStatusTransitionAsync(int id, AppointmentStatus newStatus, CancellationToken ct = default);
}
