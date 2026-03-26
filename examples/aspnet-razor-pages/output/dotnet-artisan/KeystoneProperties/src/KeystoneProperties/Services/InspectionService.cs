using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class InspectionService(ApplicationDbContext db) : IInspectionService
{
    public async Task<PaginatedList<Inspection>> GetInspectionsAsync(InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize)
    {
        var query = db.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease)
            .AsNoTracking()
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(i => i.InspectionType == type.Value);
        }
        if (unitId.HasValue)
        {
            query = query.Where(i => i.UnitId == unitId.Value);
        }
        if (fromDate.HasValue)
        {
            query = query.Where(i => i.ScheduledDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(i => i.ScheduledDate <= toDate.Value);
        }

        query = query.OrderByDescending(i => i.ScheduledDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Inspection>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Inspection?> GetByIdAsync(int id)
    {
        return await db.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease).ThenInclude(l => l!.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CreateAsync(Inspection inspection)
    {
        db.Inspections.Add(inspection);
        await db.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int id, OverallCondition condition, string? notes, bool followUpRequired)
    {
        var inspection = await db.Inspections.FindAsync(id);
        if (inspection is null)
        {
            return (false, "Inspection not found.");
        }

        if (inspection.CompletedDate.HasValue)
        {
            return (false, "Inspection is already completed.");
        }

        inspection.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;

        await db.SaveChangesAsync();
        return (true, null);
    }
}
