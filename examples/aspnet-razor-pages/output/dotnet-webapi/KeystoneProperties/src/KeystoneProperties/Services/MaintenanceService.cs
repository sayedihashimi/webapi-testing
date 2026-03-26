using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class MaintenanceService(ApplicationDbContext db, ILogger<MaintenanceService> logger)
    : IMaintenanceService
{
    public async Task<PaginatedList<MaintenanceRequest>> GetAllAsync(
        MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId,
        MaintenanceCategory? category, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.MaintenanceRequests.AsNoTracking()
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(m => m.Priority == priority.Value);
        if (propertyId.HasValue)
            query = query.Where(m => m.Unit.PropertyId == propertyId.Value);
        if (category.HasValue)
            query = query.Where(m => m.Category == category.Value);

        query = query.OrderByDescending(m => m.SubmittedDate);
        return await PaginatedList<MaintenanceRequest>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.MaintenanceRequests
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<MaintenanceRequest> CreateAsync(MaintenanceRequest request, CancellationToken ct = default)
    {
        request.SubmittedDate = DateTime.UtcNow;

        if (request.Priority == MaintenancePriority.Emergency)
        {
            request.Status = MaintenanceStatus.Assigned;
            request.AssignedDate = DateTime.UtcNow;

            // Set unit status to Maintenance
            var unit = await db.Units.FindAsync([request.UnitId], ct);
            if (unit is not null)
            {
                unit.Status = UnitStatus.Maintenance;
            }
        }

        db.MaintenanceRequests.Add(request);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Maintenance request created: {Title} (ID: {RequestId})", request.Title, request.Id);
        return request;
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(
        int id, MaintenanceStatus newStatus, string? assignedTo,
        string? completionNotes, decimal? estimatedCost, decimal? actualCost,
        CancellationToken ct = default)
    {
        var request = await db.MaintenanceRequests
            .Include(m => m.Unit)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (request is null)
            return (false, "Maintenance request not found.");

        // Validate status transitions
        var validTransitions = GetValidTransitions(request.Status);
        if (!validTransitions.Contains(newStatus))
            return (false, $"Cannot transition from {request.Status} to {newStatus}.");

        if (newStatus == MaintenanceStatus.Assigned)
        {
            if (string.IsNullOrWhiteSpace(assignedTo))
                return (false, "AssignedTo is required when assigning a request.");
            request.AssignedTo = assignedTo;
            request.AssignedDate = DateTime.UtcNow;
        }

        if (newStatus == MaintenanceStatus.Completed)
        {
            request.CompletedDate = DateTime.UtcNow;
            request.CompletionNotes = completionNotes;

            // Restore unit status if emergency
            if (request.Priority == MaintenancePriority.Emergency)
            {
                var hasActiveLease = await db.Leases
                    .AnyAsync(l => l.UnitId == request.UnitId && l.Status == LeaseStatus.Active, ct);
                request.Unit.Status = hasActiveLease ? UnitStatus.Occupied : UnitStatus.Available;
            }
        }

        if (estimatedCost.HasValue) request.EstimatedCost = estimatedCost;
        if (actualCost.HasValue) request.ActualCost = actualCost;

        request.Status = newStatus;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Maintenance request {RequestId} status changed to {Status}", id, newStatus);
        return (true, null);
    }

    public async Task<int> GetOpenRequestCountAsync(CancellationToken ct = default)
    {
        return await db.MaintenanceRequests.AsNoTracking()
            .CountAsync(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled, ct);
    }

    private static List<MaintenanceStatus> GetValidTransitions(MaintenanceStatus current) => current switch
    {
        MaintenanceStatus.Submitted => [MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled],
        MaintenanceStatus.Assigned => [MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled],
        MaintenanceStatus.InProgress => [MaintenanceStatus.Completed, MaintenanceStatus.Cancelled],
        _ => []
    };
}
