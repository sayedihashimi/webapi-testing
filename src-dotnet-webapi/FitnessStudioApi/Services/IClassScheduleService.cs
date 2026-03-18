using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(DateOnly? date, int? classTypeId, int? instructorId, bool? available, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct);
    Task<List<ClassRosterResponse>> GetRosterAsync(int id, CancellationToken ct);
    Task<List<ClassRosterResponse>> GetWaitlistAsync(int id, CancellationToken ct);
    Task<List<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct);
}
