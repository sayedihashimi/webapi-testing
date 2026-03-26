using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class MaintenanceService(ApplicationDbContext context, ILogger<MaintenanceService> logger) : IMaintenanceService
{
    public async Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(
        MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId,
        MaintenanceCategory? category, int pageNumber, int pageSize)
    {
        var query = context.MaintenanceRequests
            .Include(mr => mr.Unit)
                .ThenInclude(u => u.Property)
            .Include(mr => mr.Tenant)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(mr => mr.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(mr => mr.Priority == priority.Value);
        }

        if (propertyId.HasValue)
        {
            query = query.Where(mr => mr.Unit.PropertyId == propertyId.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(mr => mr.Category == category.Value);
        }

        query = query.OrderByDescending(mr => mr.SubmittedDate);

        return await PaginatedList<MaintenanceRequest>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<MaintenanceRequest?> GetRequestByIdAsync(int id)
    {
        return await context.MaintenanceRequests
            .Include(mr => mr.Unit)
                .ThenInclude(u => u.Property)
            .Include(mr => mr.Tenant)
            .FirstOrDefaultAsync(mr => mr.Id == id);
    }

    public async Task<MaintenanceRequest?> GetRequestWithDetailsAsync(int id)
    {
        return await context.MaintenanceRequests
            .Include(mr => mr.Unit)
                .ThenInclude(u => u.Property)
            .Include(mr => mr.Tenant)
            .FirstOrDefaultAsync(mr => mr.Id == id);
    }

    public async Task CreateRequestAsync(MaintenanceRequest request)
    {
        request.SubmittedDate = DateTime.UtcNow;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        context.MaintenanceRequests.Add(request);

        // For emergency priority, set unit to Maintenance
        if (request.Priority == MaintenancePriority.Emergency)
        {
            var unit = await context.Units.FindAsync(request.UnitId);
            if (unit is not null)
            {
                unit.Status = UnitStatus.Maintenance;
                unit.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Created maintenance request {RequestId}: {Title} for unit {UnitId}",
            request.Id, request.Title, request.UnitId);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(
        int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes,
        decimal? estimatedCost, decimal? actualCost)
    {
        var request = await context.MaintenanceRequests
            .Include(mr => mr.Unit)
            .FirstOrDefaultAsync(mr => mr.Id == id);

        if (request is null)
        {
            return (false, "Maintenance request not found.");
        }

        // Validate workflow transitions
        var validTransition = (request.Status, newStatus) switch
        {
            (MaintenanceStatus.Submitted, MaintenanceStatus.Assigned) => true,
            (MaintenanceStatus.Submitted, MaintenanceStatus.Cancelled) => true,
            (MaintenanceStatus.Assigned, MaintenanceStatus.InProgress) => true,
            (MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled) => true,
            (MaintenanceStatus.InProgress, MaintenanceStatus.Completed) => true,
            (MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled) => true,
            _ => false
        };

        if (!validTransition)
        {
            return (false, $"Cannot transition from {request.Status} to {newStatus}.");
        }

        // Assign
        if (newStatus == MaintenanceStatus.Assigned)
        {
            if (string.IsNullOrWhiteSpace(assignedTo))
            {
                return (false, "AssignedTo is required when assigning a maintenance request.");
            }

            request.AssignedTo = assignedTo;
            request.AssignedDate = DateTime.UtcNow;
        }

        // Complete
        if (newStatus == MaintenanceStatus.Completed)
        {
            request.CompletedDate = DateTime.UtcNow;
            request.CompletionNotes = completionNotes;

            // Restore unit status if it was set to Maintenance for an emergency
            if (request.Priority == MaintenancePriority.Emergency && request.Unit.Status == UnitStatus.Maintenance)
            {
                var hasActiveLease = await context.Leases
                    .AnyAsync(l => l.UnitId == request.UnitId && l.Status == LeaseStatus.Active);

                request.Unit.Status = hasActiveLease ? UnitStatus.Occupied : UnitStatus.Available;
                request.Unit.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (estimatedCost.HasValue)
        {
            request.EstimatedCost = estimatedCost.Value;
        }

        if (actualCost.HasValue)
        {
            request.ActualCost = actualCost.Value;
        }

        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Updated maintenance request {RequestId} status to {Status}", request.Id, newStatus);
        return (true, null);
    }

    public async Task<int> GetOpenRequestCountAsync()
    {
        return await context.MaintenanceRequests
            .CountAsync(mr => mr.Status != MaintenanceStatus.Completed && mr.Status != MaintenanceStatus.Cancelled);
    }
}
