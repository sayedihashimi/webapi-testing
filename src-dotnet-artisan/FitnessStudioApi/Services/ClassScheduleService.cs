using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResult<ClassScheduleDto>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId,
        bool? available, int page, int pageSize)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);
        if (classTypeId.HasValue) query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue) query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (available == true) query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(cs => ToDto(cs))
            .ToListAsync();

        return new PaginatedResult<ClassScheduleDto>(items, total, page, pageSize);
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(int id)
    {
        var cs = await db.ClassSchedules
            .Include(c => c.ClassType).Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
        return cs is null ? null : ToDto(cs);
    }

    public async Task<(ClassScheduleDto? Result, string? Error)> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await db.ClassTypes.FindAsync(dto.ClassTypeId);
        if (classType is null) return (null, "Class type not found.");

        var instructor = await db.Instructors.FindAsync(dto.InstructorId);
        if (instructor is null) return (null, "Instructor not found.");
        if (!instructor.IsActive) return (null, "Instructor is not active.");

        if (dto.EndTime <= dto.StartTime)
            return (null, "End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime && cs.EndTime > dto.StartTime);
        if (hasConflict) return (null, "Instructor has a schedule conflict during this time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity,
            Room = dto.Room
        };
        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync();

        logger.LogInformation("Scheduled class {ClassTypeId} at {StartTime} (Id={Id})", dto.ClassTypeId, dto.StartTime, schedule.Id);
        var result = await db.ClassSchedules
            .Include(c => c.ClassType).Include(c => c.Instructor)
            .FirstAsync(c => c.Id == schedule.Id);
        return (ToDto(result), null);
    }

    public async Task<(ClassScheduleDto? Result, string? Error)> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await db.ClassSchedules.Include(c => c.ClassType).Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (schedule is null) return (null, "Class schedule not found.");
        if (schedule.Status != ClassScheduleStatus.Scheduled)
            return (null, "Can only update scheduled classes.");

        if (dto.EndTime <= dto.StartTime)
            return (null, "End time must be after start time.");

        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Id != id &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime && cs.EndTime > dto.StartTime);
        if (hasConflict) return (null, "Instructor has a schedule conflict during this time.");

        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated class schedule {Id}", id);

        var result = await db.ClassSchedules
            .Include(c => c.ClassType).Include(c => c.Instructor)
            .FirstAsync(c => c.Id == id);
        return (ToDto(result), null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id);

        if (schedule is null) return (false, "Class schedule not found.");
        if (schedule.Status == ClassScheduleStatus.Cancelled)
            return (false, "Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }
        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled class schedule {Id}, reason: {Reason}", id, dto.Reason);
        return (true, null);
    }

    public async Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id)
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryDto(b.Id, b.MemberId, b.Member.FirstName + " " + b.Member.LastName, b.BookingDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id)
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryDto(b.Id, b.MemberId, b.Member.FirstName + " " + b.Member.LastName, b.WaitlistPosition, b.BookingDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClassScheduleDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await db.ClassSchedules
            .Include(cs => cs.ClassType).Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now && cs.StartTime <= weekFromNow &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => ToDto(cs))
            .ToListAsync();
    }

    internal static ClassScheduleDto ToDto(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        cs.Room, cs.Status, cs.CancellationReason,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.CreatedAt, cs.UpdatedAt);
}
