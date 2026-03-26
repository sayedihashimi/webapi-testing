using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class AttendeeService(SparkEventsDbContext db, ILogger<AttendeeService> logger)
    : IAttendeeService
{
    public async Task<PaginatedList<Attendee>> GetAllAsync(string? search, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Attendees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.FirstName.Contains(search) ||
                a.LastName.Contains(search) ||
                a.Email.Contains(search));
        }

        query = query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName);
        return await PaginatedList<Attendee>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<List<Attendee>> GetAllForDropdownAsync(CancellationToken ct = default)
    {
        return await db.Attendees.AsNoTracking()
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .ToListAsync(ct);
    }

    public async Task<Attendee?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Attendees.FindAsync([id], ct);
    }

    public async Task<Attendee?> GetByIdWithRegistrationsAsync(int id, CancellationToken ct = default)
    {
        return await db.Attendees
            .Include(a => a.Registrations)
                .ThenInclude(r => r.Event)
                    .ThenInclude(e => e.EventCategory)
            .Include(a => a.Registrations)
                .ThenInclude(r => r.TicketType)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<Attendee> CreateAsync(Attendee attendee, CancellationToken ct = default)
    {
        attendee.CreatedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;
        db.Attendees.Add(attendee);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created attendee {AttendeeName} (ID: {AttendeeId})", attendee.FullName, attendee.Id);
        return attendee;
    }

    public async Task UpdateAsync(Attendee attendee, CancellationToken ct = default)
    {
        attendee.UpdatedAt = DateTime.UtcNow;
        db.Attendees.Update(attendee);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated attendee {AttendeeId}", attendee.Id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var query = db.Attendees.Where(a => a.Email == email);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Attendee?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await db.Attendees.FirstOrDefaultAsync(a => a.Email == email, ct);
    }
}
