using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IInstructorService
{
    Task<PagedResponse<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct);
    Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct);
    Task<List<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct);
}
