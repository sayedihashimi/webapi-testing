using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IMaintenanceService
{
    Task<PaginatedList<MaintenanceRequest>> GetAllAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<MaintenanceRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MaintenanceRequest> CreateAsync(MaintenanceRequest request, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost, CancellationToken ct = default);
    Task<int> GetOpenRequestCountAsync(CancellationToken ct = default);
}
