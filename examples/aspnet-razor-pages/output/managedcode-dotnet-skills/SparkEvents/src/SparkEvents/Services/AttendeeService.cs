using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class AttendeeService(SparkEventsDbContext context, ILogger<AttendeeService> logger) : IAttendeeService
{
    public async Task<(List<Attendee> Items, int TotalCount)> GetFilteredAsync(
        string? search, int page, int pageSize)
    {
        var query = context.Attendees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(term) ||
                a.LastName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Attendee?> GetByIdAsync(int id)
    {
        return await context.Attendees
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Attendee?> GetByIdWithRegistrationsAsync(int id)
    {
        return await context.Attendees
            .AsNoTracking()
            .Include(a => a.Registrations)
                .ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Attendee> CreateAsync(Attendee attendee)
    {
        attendee.CreatedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;

        context.Attendees.Add(attendee);
        await context.SaveChangesAsync();

        logger.LogInformation("Created attendee '{FirstName} {LastName}' with Id {Id}",
            attendee.FirstName, attendee.LastName, attendee.Id);
        return attendee;
    }

    public async Task UpdateAsync(Attendee attendee)
    {
        attendee.UpdatedAt = DateTime.UtcNow;

        context.Attendees.Update(attendee);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated attendee '{FirstName} {LastName}' (Id: {Id})",
            attendee.FirstName, attendee.LastName, attendee.Id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await context.Attendees
            .AsNoTracking()
            .AnyAsync(a => a.Email == email && (!excludeId.HasValue || a.Id != excludeId.Value));
    }

    public async Task<List<Attendee>> GetAllAsync()
    {
        return await context.Attendees
            .AsNoTracking()
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToListAsync();
    }
}
