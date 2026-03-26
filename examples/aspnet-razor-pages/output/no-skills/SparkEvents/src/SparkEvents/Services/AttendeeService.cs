using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class AttendeeService : IAttendeeService
{
    private readonly SparkEventsDbContext _context;

    public AttendeeService(SparkEventsDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Attendee>> GetAttendeesAsync(string? search, int pageIndex = 1, int pageSize = 10)
    {
        var query = _context.Attendees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.FirstName.Contains(search) ||
                a.LastName.Contains(search) ||
                a.Email.Contains(search));
        }

        query = query.OrderBy(a => a.LastName).ThenBy(a => a.FirstName);
        return await PaginatedList<Attendee>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<Attendee?> GetAttendeeByIdAsync(int id)
    {
        return await _context.Attendees.FindAsync(id);
    }

    public async Task<Attendee?> GetAttendeeByEmailAsync(string email)
    {
        return await _context.Attendees.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<Attendee> CreateAttendeeAsync(Attendee attendee)
    {
        attendee.CreatedAt = DateTime.UtcNow;
        attendee.UpdatedAt = DateTime.UtcNow;
        _context.Attendees.Add(attendee);
        await _context.SaveChangesAsync();
        return attendee;
    }

    public async Task UpdateAttendeeAsync(Attendee attendee)
    {
        attendee.UpdatedAt = DateTime.UtcNow;
        _context.Attendees.Update(attendee);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Attendee>> GetAllAttendeesAsync()
    {
        return await _context.Attendees.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToListAsync();
    }

    public async Task<List<Registration>> GetAttendeeRegistrationsAsync(int attendeeId)
    {
        return await _context.Registrations
            .Include(r => r.Event)
            .Include(r => r.TicketType)
            .Where(r => r.AttendeeId == attendeeId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();
    }
}
