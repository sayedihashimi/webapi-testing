using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IInstructorService
{
    Task<List<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, CancellationToken ct = default);
    Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default);
    Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct = default);
    Task<List<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
