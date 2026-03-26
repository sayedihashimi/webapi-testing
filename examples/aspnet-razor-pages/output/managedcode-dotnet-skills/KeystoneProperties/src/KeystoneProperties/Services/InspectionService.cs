using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class InspectionService(ApplicationDbContext context, ILogger<InspectionService> logger) : IInspectionService
{
    public async Task<PaginatedList<Inspection>> GetInspectionsAsync(
        InspectionType? type, int? unitId, DateOnly? startDate, DateOnly? endDate,
        int pageNumber, int pageSize)
    {
        var query = context.Inspections
            .Include(i => i.Unit)
                .ThenInclude(u => u.Property)
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

        if (startDate.HasValue)
        {
            query = query.Where(i => i.ScheduledDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.ScheduledDate <= endDate.Value);
        }

        query = query.OrderByDescending(i => i.ScheduledDate);

        return await PaginatedList<Inspection>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Inspection?> GetInspectionByIdAsync(int id)
    {
        return await context.Inspections
            .Include(i => i.Unit)
                .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Inspection?> GetInspectionWithDetailsAsync(int id)
    {
        return await context.Inspections
            .Include(i => i.Unit)
                .ThenInclude(u => u.Property)
            .Include(i => i.Lease)
                .ThenInclude(l => l!.Tenant)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task CreateInspectionAsync(Inspection inspection)
    {
        inspection.CreatedAt = DateTime.UtcNow;
        context.Inspections.Add(inspection);
        await context.SaveChangesAsync();

        logger.LogInformation("Created inspection {InspectionId} for unit {UnitId}, scheduled {ScheduledDate}",
            inspection.Id, inspection.UnitId, inspection.ScheduledDate);
    }

    public async Task CompleteInspectionAsync(int id, OverallCondition condition, string? notes, bool followUpRequired)
    {
        var inspection = await context.Inspections.FindAsync(id);

        if (inspection is null)
        {
            return;
        }

        inspection.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;
        await context.SaveChangesAsync();

        logger.LogInformation("Completed inspection {InspectionId} with condition {Condition}", id, condition);
    }
}
