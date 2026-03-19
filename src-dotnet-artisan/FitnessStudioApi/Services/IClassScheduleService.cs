using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleResponse>> GetAllAsync(
        DateOnly? date, int? classTypeId, int? instructorId, bool? hasAvailability,
        int page, int pageSize, CancellationToken ct = default);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct = default);
    Task<ClassScheduleResponse> CancelClassAsync(int id, string reason, CancellationToken ct = default);
    Task<List<ClassRosterEntry>> GetRosterAsync(int classId, CancellationToken ct = default);
    Task<List<ClassWaitlistEntry>> GetWaitlistAsync(int classId, CancellationToken ct = default);
    Task<List<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct = default);
}
