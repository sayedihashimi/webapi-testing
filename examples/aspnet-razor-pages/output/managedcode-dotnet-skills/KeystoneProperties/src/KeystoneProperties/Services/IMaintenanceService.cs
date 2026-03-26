using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IMaintenanceService
{
    Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize);
    Task<MaintenanceRequest?> GetRequestByIdAsync(int id);
    Task<MaintenanceRequest?> GetRequestWithDetailsAsync(int id);
    Task CreateRequestAsync(MaintenanceRequest request);
    Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost);
    Task<int> GetOpenRequestCountAsync();
}
