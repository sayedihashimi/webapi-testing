using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IInspectionService
{
    Task<PaginatedList<Inspection>> GetInspectionsAsync(InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize);
    Task<Inspection?> GetByIdAsync(int id);
    Task CreateAsync(Inspection inspection);
    Task<(bool Success, string? Error)> CompleteAsync(int id, OverallCondition condition, string? notes, bool followUpRequired);
}
