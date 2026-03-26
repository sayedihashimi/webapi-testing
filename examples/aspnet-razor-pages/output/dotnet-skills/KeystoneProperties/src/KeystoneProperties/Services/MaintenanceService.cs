using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneProperties.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(ApplicationDbContext context, ILogger<MaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<MaintenanceRequest>> GetRequestsAsync(
        MaintenanceStatus? status,
        MaintenancePriority? priority,
        int? propertyId,
        MaintenanceCategory? category,
        int pageNumber,
        int pageSize)
    {
        var query = _context.MaintenanceRequests
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .Include(r => r.Tenant)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        if (propertyId.HasValue)
            query = query.Where(r => r.Unit.PropertyId == propertyId.Value);

        if (category.HasValue)
            query = query.Where(r => r.Category == category.Value);

        query = query.OrderByDescending(r => r.SubmittedDate);

        return await PaginatedList<MaintenanceRequest>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(int id)
    {
        return await _context.MaintenanceRequests.FindAsync(id);
    }

    public async Task<MaintenanceRequest?> GetWithDetailsAsync(int id)
    {
        return await _context.MaintenanceRequests
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .Include(r => r.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task CreateAsync(MaintenanceRequest request)
    {
        if (request.Priority == MaintenancePriority.Emergency &&
            string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            throw new InvalidOperationException("Emergency maintenance requests must have an assigned technician.");
        }

        _context.MaintenanceRequests.Add(request);

        if (request.Priority == MaintenancePriority.Emergency)
        {
            var unit = await _context.Units.FindAsync(request.UnitId);
            if (unit != null)
            {
                unit.Status = UnitStatus.Maintenance;
                _logger.LogInformation("Unit {UnitId} status changed to Maintenance due to emergency request.", unit.Id);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Maintenance request {Id} created with priority {Priority}.", request.Id, request.Priority);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(
        int id,
        MaintenanceStatus newStatus,
        string? assignedTo,
        string? completionNotes,
        decimal? estimatedCost,
        decimal? actualCost)
    {
        var request = await _context.MaintenanceRequests
            .Include(r => r.Unit)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return (false, "Maintenance request not found.");

        var validStatuses = await GetValidNextStatuses(request.Status);
        if (!validStatuses.Contains(newStatus))
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
        }

        if (completionNotes != null)
            request.CompletionNotes = completionNotes;

        if (estimatedCost.HasValue)
            request.EstimatedCost = estimatedCost;

        if (actualCost.HasValue)
            request.ActualCost = actualCost;

        var previousStatus = request.Status;
        request.Status = newStatus;

        // Handle emergency request completion — restore unit status if no other open emergencies
        if (newStatus == MaintenanceStatus.Completed &&
            request.Priority == MaintenancePriority.Emergency)
        {
            var hasOtherOpenEmergencies = await _context.MaintenanceRequests
                .AnyAsync(r => r.UnitId == request.UnitId
                    && r.Id != request.Id
                    && r.Priority == MaintenancePriority.Emergency
                    && r.Status != MaintenanceStatus.Completed
                    && r.Status != MaintenanceStatus.Cancelled);

            if (!hasOtherOpenEmergencies && request.Unit != null)
            {
                var hasActiveLease = await _context.Leases
                    .AnyAsync(l => l.UnitId == request.UnitId && l.Status == LeaseStatus.Active);

                request.Unit.Status = hasActiveLease ? UnitStatus.Occupied : UnitStatus.Available;
                _logger.LogInformation(
                    "Unit {UnitId} status restored to {Status} after emergency request {RequestId} completed.",
                    request.UnitId, request.Unit.Status, request.Id);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Maintenance request {Id} status changed from {OldStatus} to {NewStatus}.",
            id, previousStatus, newStatus);

        return (true, null);
    }

    public async Task<int> GetOpenCountAsync()
    {
        return await _context.MaintenanceRequests
            .CountAsync(r => r.Status != MaintenanceStatus.Completed
                          && r.Status != MaintenanceStatus.Cancelled);
    }

    public Task<List<MaintenanceStatus>> GetValidNextStatuses(MaintenanceStatus current)
    {
        var valid = current switch
        {
            MaintenanceStatus.Submitted => new List<MaintenanceStatus>
                { MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled },
            MaintenanceStatus.Assigned => new List<MaintenanceStatus>
                { MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled },
            MaintenanceStatus.InProgress => new List<MaintenanceStatus>
                { MaintenanceStatus.Completed, MaintenanceStatus.Cancelled },
            _ => new List<MaintenanceStatus>()
        };

        return Task.FromResult(valid);
    }
}
