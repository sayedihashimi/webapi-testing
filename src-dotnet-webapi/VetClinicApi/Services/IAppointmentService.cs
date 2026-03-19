using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct);
    Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct);
}
