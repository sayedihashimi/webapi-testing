using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IClassTypeService
{
    Task<PaginatedResponse<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, int page, int pageSize, CancellationToken ct);
    Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct);
    Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct);
}
