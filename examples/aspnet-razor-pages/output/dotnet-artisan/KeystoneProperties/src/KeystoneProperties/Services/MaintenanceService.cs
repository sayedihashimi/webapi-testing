using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class MaintenanceService(ApplicationDbContext db, ILogger<MaintenanceService> logger) : IMaintenanceService
{
    public async Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(MaintenanceStatus? status, MaintenancePriority? priority, int? propertyId, MaintenanceCategory? category, int pageNumber, int pageSize)
    {
        var query = db.MaintenanceRequests
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }
        if (priority.HasValue)
        {
            query = query.Where(m => m.Priority == priority.Value);
        }
        if (propertyId.HasValue)
        {
            query = query.Where(m => m.Unit.PropertyId == propertyId.Value);
        }
        if (category.HasValue)
        {
            query = query.Where(m => m.Category == category.Value);
        }

        query = query.OrderByDescending(m => m.SubmittedDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<MaintenanceRequest>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(int id)
    {
        return await db.MaintenanceRequests
            .Include(m => m.Unit).ThenInclude(u => u.Property)
            .Include(m => m.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateAsync(MaintenanceRequest request)
    {
        request.SubmittedDate = DateTime.UtcNow;

        if (request.Priority == MaintenancePriority.Emergency && !string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            request.Status = MaintenanceStatus.Assigned;
            request.AssignedDate = DateTime.UtcNow;

            // Set unit to Maintenance status
            var unit = await db.Units.FindAsync(request.UnitId);
            if (unit is not null)
            {
                unit.Status = UnitStatus.Maintenance;
            }
        }

        db.MaintenanceRequests.Add(request);
        await db.SaveChangesAsync();
        logger.LogInformation("Maintenance request created: {Title} (ID: {RequestId}), Priority: {Priority}", request.Title, request.Id, request.Priority);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, MaintenanceStatus newStatus, string? assignedTo, string? completionNotes, decimal? estimatedCost, decimal? actualCost)
    {
        var request = await db.MaintenanceRequests.Include(m => m.Unit).FirstOrDefaultAsync(m => m.Id == id);
        if (request is null)
        {
            return (false, "Maintenance request not found.");
        }

        // Validate status transitions
        var validTransitions = request.Status switch
        {
            MaintenanceStatus.Submitted => new[] { MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled },
            MaintenanceStatus.Assigned => new[] { MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled },
            MaintenanceStatus.InProgress => new[] { MaintenanceStatus.Completed, MaintenanceStatus.Cancelled },
            _ => Array.Empty<MaintenanceStatus>()
        };

        if (!validTransitions.Contains(newStatus))
        {
            return (false, $"Cannot transition from {request.Status} to {newStatus}.");
        }

        if (newStatus == MaintenanceStatus.Assigned)
        {
            if (string.IsNullOrWhiteSpace(assignedTo))
            {
                return (false, "AssignedTo is required when assigning a request.");
            }
            request.AssignedTo = assignedTo;
            request.AssignedDate = DateTime.UtcNow;
        }

        if (newStatus == MaintenanceStatus.Completed)
        {
            request.CompletedDate = DateTime.UtcNow;
            request.CompletionNotes = completionNotes;

            // If emergency request completed, restore unit status
            if (request.Priority == MaintenancePriority.Emergency)
            {
                var hasActiveLease = await db.Leases.AnyAsync(l => l.UnitId == request.UnitId && l.Status == LeaseStatus.Active);
                request.Unit.Status = hasActiveLease ? UnitStatus.Occupied : UnitStatus.Available;
            }
        }

        if (estimatedCost.HasValue)
        {
            request.EstimatedCost = estimatedCost;
        }
        if (actualCost.HasValue)
        {
            request.ActualCost = actualCost;
        }

        request.Status = newStatus;
        await db.SaveChangesAsync();

        logger.LogInformation("Maintenance request {RequestId} status changed to {Status}", request.Id, newStatus);
        return (true, null);
    }

    public async Task<int> GetOpenCountAsync()
    {
        return await db.MaintenanceRequests.CountAsync(m =>
            m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled);
    }
}
