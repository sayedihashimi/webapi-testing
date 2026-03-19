using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponse>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken cancellationToken);
    Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<(AppointmentResponse? Result, string? Error)> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);
    Task<(AppointmentResponse? Result, string? Error, bool NotFound)> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string? Error, bool NotFound)> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken cancellationToken);
}
