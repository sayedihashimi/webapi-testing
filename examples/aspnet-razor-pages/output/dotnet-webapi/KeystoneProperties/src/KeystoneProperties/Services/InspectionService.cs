using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class InspectionService(ApplicationDbContext db, ILogger<InspectionService> logger)
    : IInspectionService
{
    public async Task<PaginatedList<Inspection>> GetAllAsync(
        InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Inspections.AsNoTracking()
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease)
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
        return await PaginatedList<Inspection>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Inspection?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease).ThenInclude(l => l!.Tenant)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Inspection> CreateAsync(Inspection inspection, CancellationToken ct = default)
    {
        db.Inspections.Add(inspection);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Inspection scheduled: {Type} for Unit {UnitId} on {Date}", inspection.InspectionType, inspection.UnitId, inspection.ScheduledDate);
        return inspection;
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(
        int id, DateOnly completedDate, OverallCondition condition,
        string? notes, bool followUpRequired, CancellationToken ct = default)
    {
        var inspection = await db.Inspections.FindAsync([id], ct);
        if (inspection is null)
            return (false, "Inspection not found.");

        if (inspection.CompletedDate.HasValue)
            return (false, "Inspection has already been completed.");

        inspection.CompletedDate = completedDate;
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Inspection completed: ID {InspectionId}, Condition: {Condition}", id, condition);
        return (true, null);
    }
}
