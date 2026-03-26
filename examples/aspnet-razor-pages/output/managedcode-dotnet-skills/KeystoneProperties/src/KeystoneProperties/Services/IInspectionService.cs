using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IInspectionService
{
    Task<PaginatedList<Inspection>> GetInspectionsAsync(InspectionType? type, int? unitId, DateOnly? startDate, DateOnly? endDate, int pageNumber, int pageSize);
    Task<Inspection?> GetInspectionByIdAsync(int id);
    Task<Inspection?> GetInspectionWithDetailsAsync(int id);
    Task CreateInspectionAsync(Inspection inspection);
    Task CompleteInspectionAsync(int id, OverallCondition condition, string? notes, bool followUpRequired);
}
