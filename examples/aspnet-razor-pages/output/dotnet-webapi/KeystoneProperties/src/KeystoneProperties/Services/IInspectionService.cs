using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IInspectionService
{
    Task<PaginatedList<Inspection>> GetAllAsync(InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Inspection?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Inspection> CreateAsync(Inspection inspection, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CompleteAsync(int id, DateOnly completedDate, OverallCondition condition, string? notes, bool followUpRequired, CancellationToken ct = default);
}
