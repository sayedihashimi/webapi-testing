using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct);
    Task<IReadOnlyList<ClassRosterEntryResponse>> GetRosterAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<WaitlistEntryResponse>> GetWaitlistAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct);
}
