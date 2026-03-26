using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class InspectionService : IInspectionService
{
    private readonly ApplicationDbContext _context;

    public InspectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Inspection> Items, int TotalCount)> GetInspectionsAsync(
        InspectionType? type, int? unitId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize)
    {
        var query = _context.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .AsQueryable();

        if (type.HasValue) query = query.Where(i => i.InspectionType == type.Value);
        if (unitId.HasValue) query = query.Where(i => i.UnitId == unitId.Value);
        if (fromDate.HasValue) query = query.Where(i => i.ScheduledDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(i => i.ScheduledDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.ScheduledDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Inspection?> GetByIdAsync(int id) =>
        await _context.Inspections
            .Include(i => i.Unit).ThenInclude(u => u.Property)
            .Include(i => i.Lease)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Inspection?> GetWithDetailsAsync(int id) => await GetByIdAsync(id);

    public async Task CreateAsync(Inspection inspection)
    {
        inspection.CreatedAt = DateTime.UtcNow;
        _context.Inspections.Add(inspection);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(
        int id, OverallCondition condition, string? notes, bool followUpRequired)
    {
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null) return (false, "Inspection not found.");
        if (inspection.CompletedDate.HasValue) return (false, "Inspection is already completed.");

        inspection.CompletedDate = DateOnly.FromDateTime(DateTime.Today);
        inspection.OverallCondition = condition;
        inspection.Notes = notes;
        inspection.FollowUpRequired = followUpRequired;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
