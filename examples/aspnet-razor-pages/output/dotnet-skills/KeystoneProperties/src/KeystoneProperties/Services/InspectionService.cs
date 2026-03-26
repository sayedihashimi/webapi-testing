using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneProperties.Services;

public class InspectionService : IInspectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(ApplicationDbContext context, ILogger<InspectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Inspection>> GetInspectionsAsync(
        InspectionType? type,
        int? unitId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Inspections
            .Include(i => i.Unit)
                .ThenInclude(u => u.Property)
            .AsNoTracking()
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(i => i.InspectionType == type.Value);

        if (unitId.HasValue)
            query = query.Where(i => i.UnitId == unitId.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.ScheduledDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.ScheduledDate <= toDate.Value);

        query = query.OrderByDescending(i => i.ScheduledDate);

        return await PaginatedList<Inspection>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Inspection?> GetByIdAsync(int id)
    {
        return await _context.Inspections.FindAsync(id);
    }

    public async Task<Inspection?> GetWithDetailsAsync(int id)
    {
        return await _context.Inspections
            .Include(i => i.Unit)
                .ThenInclude(u => u.Property)
            .Include(i => i.Lease!)
                .ThenInclude(l => l.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CreateAsync(Inspection inspection)
    {
        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Inspection {Id} created for unit {UnitId}.", inspection.Id, inspection.UnitId);
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(
        int id,
        OverallCondition condition,
        string? notes,
        bool followUpRequired)
    {
        var inspection = await _context.Inspections.FindAsync(id);

        if (inspection == null)
            return (false, "Inspection not found.");

        if (inspection.CompletedDate.HasValue)
            return (false, "Inspection has already been completed.");

        inspection.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Inspection {Id} completed with condition {Condition}.", id, condition);

        return (true, null);
    }
}
