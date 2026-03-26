using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class AttendeeService : IAttendeeService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<AttendeeService> _logger;

    public AttendeeService(SparkEventsDbContext db, ILogger<AttendeeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedList<Attendee>> GetAttendeesAsync(string? search, int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.Attendees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(term) ||
                a.LastName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term));
        }

        query = query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Attendee>(items, count, pageNumber, pageSize);
    }

    public async Task<Attendee?> GetAttendeeByIdAsync(int id) =>
        await _db.Attendees
            .AsNoTracking()
            .Include(a => a.Registrations)
                .ThenInclude(r => r.Event)
            .Include(a => a.Registrations)
                .ThenInclude(r => r.TicketType)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Attendee?> GetAttendeeByEmailAsync(string email) =>
        await _db.Attendees.FirstOrDefaultAsync(a => a.Email == email);

    public async Task<Attendee> CreateAttendeeAsync(Attendee attendee)
    {
        attendee.CreatedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;
        _db.Attendees.Add(attendee);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created attendee: {Name}", attendee.FullName);
        return attendee;
    }

    public async Task UpdateAttendeeAsync(Attendee attendee)
    {
        attendee.UpdatedAt = DateTime.UtcNow;
        _db.Attendees.Update(attendee);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Attendee>> GetAllAttendeesAsync() =>
        await _db.Attendees.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToListAsync();
}
