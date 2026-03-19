using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(AppDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId,
        bool? available, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(cs => cs.StartTime >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(cs => cs.StartTime <= to);
        }

        if (classTypeId.HasValue)
        {
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(cs => cs.InstructorId == instructorId.Value);
        }

        if (available == true)
        {
            query = query.Where(cs =>
                cs.Status == ClassScheduleStatus.Scheduled &&
                cs.CurrentEnrollment < cs.Capacity);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return new PaginatedResponse<ClassScheduleResponse>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        return schedule is null ? null : MapToResponse(schedule);
    }

    public async Task<(ClassScheduleResponse? Result, string? Error)> CreateAsync(
        CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct);
        if (classType is null)
        {
            return (null, "Class type not found");
        }

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct);
        if (instructor is null)
        {
            return (null, "Instructor not found");
        }

        var duration = request.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = request.Capacity ?? classType.DefaultCapacity;
        var endTime = request.StartTime.AddMinutes(duration);

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < endTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            return (null, "Instructor has a scheduling conflict at this time");
        }

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = request.Room
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        await db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        logger.LogInformation("Scheduled class {ClassName} at {StartTime} in {Room}",
            classType.Name, schedule.StartTime, schedule.Room);

        return (MapToResponse(schedule), null);
    }

    public async Task<(ClassScheduleResponse? Result, string? Error)> UpdateAsync(
        int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null)
        {
            return (null, "Class schedule not found");
        }

        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct);
        if (classType is null)
        {
            return (null, "Class type not found");
        }

        var duration = request.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = request.Capacity ?? classType.DefaultCapacity;
        var endTime = request.StartTime.AddMinutes(duration);

        // Check instructor conflicts (exclude this class)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < endTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            return (null, "Instructor has a scheduling conflict at this time");
        }

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = endTime;
        schedule.Capacity = capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await db.Entry(schedule).Reference(s => s.ClassType).LoadAsync(ct);
        await db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        return (MapToResponse(schedule), null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules.FindAsync([id], ct);
        if (schedule is null)
        {
            return (false, "Class schedule not found");
        }

        if (schedule.Status is ClassScheduleStatus.Cancelled)
        {
            return (false, "Class is already cancelled");
        }

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = "Class cancelled by studio";
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all bookings for this class
        var bookings = await db.Bookings
            .Where(b => b.ClassScheduleId == id &&
                b.Status != BookingStatus.Cancelled)
            .ToListAsync(ct);

        foreach (var booking in bookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled class {ClassId}, {BookingCount} bookings cancelled", id, bookings.Count);
        return (true, null);
    }

    public async Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int id, CancellationToken ct)
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterEntry(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.Status.ToString(), b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WaitlistEntry>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntry(
                b.Id, b.MemberId,
                b.Member.FirstName + " " + b.Member.LastName,
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        return await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs =>
                cs.Status == ClassScheduleStatus.Scheduled &&
                cs.StartTime > now &&
                cs.StartTime <= weekFromNow &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) =>
        new(cs.Id, cs.ClassTypeId, cs.ClassType.Name,
            cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
            cs.StartTime, cs.EndTime, cs.Capacity,
            cs.CurrentEnrollment, cs.WaitlistCount,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
            cs.Room, cs.Status.ToString(), cs.CancellationReason);
}
