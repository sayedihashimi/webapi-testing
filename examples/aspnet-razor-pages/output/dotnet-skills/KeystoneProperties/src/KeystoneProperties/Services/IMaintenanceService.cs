using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IMaintenanceService
{
    Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize);
    Task<MaintenanceRequest?> GetByIdAsync(int id);
    Task<MaintenanceRequest?> GetWithDetailsAsync(int id);
    Task CreateAsync(MaintenanceRequest request);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost);
    Task<int> GetOpenCountAsync();
    Task<List<MaintenanceStatus>> GetValidNextStatuses(MaintenanceStatus current);
}
