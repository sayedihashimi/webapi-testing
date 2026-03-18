using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(StudioDbContext db, ILogger<ClassScheduleService> logger)
{
    public async Task<PagedResponse<ClassScheduleResponse>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(cs => cs.StartTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(cs => cs.StartTime <= to.Value);
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
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity &&
                                      cs.Status == ClassScheduleStatus.Scheduled);
        }

        var totalCount = await query.CountAsync(ct);
        var schedules = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = schedules.Select(ToResponse).ToList();
        return new PagedResponse<ClassScheduleResponse>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        return schedule is null ? null : ToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new InvalidOperationException("Class type not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new InvalidOperationException("Instructor not found.");

        // Check instructor conflict
        var hasConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < request.EndTime &&
                           cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");
        }

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            Room = request.Room
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        schedule.ClassType = classType;
        schedule.Instructor = instructor;

        logger.LogInformation("Class schedule {ScheduleId} created for {ClassName}", schedule.Id, classType.Name);
        return ToResponse(schedule);
    }

    public async Task<(string? Error, ClassScheduleResponse? Result)> UpdateAsync(
        int id, UpdateClassScheduleRequest request, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null)
        {
            return ("not_found", null);
        }

        // Check instructor conflict (excluding self)
        var hasConflict = await db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == request.InstructorId &&
                           cs.Id != id &&
                           cs.Status != ClassScheduleStatus.Cancelled &&
                           cs.StartTime < request.EndTime &&
                           cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            return ("instructor_conflict", null);
        }

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct);
        if (instructor is null)
        {
            return ("instructor_not_found", null);
        }

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.Instructor = instructor;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return (null, ToResponse(schedule));
    }

    public async Task<string?> CancelClassAsync(int id, string reason, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null)
        {
            return "not_found";
        }

        if (schedule.Status != ClassScheduleStatus.Scheduled)
        {
            return "not_scheduled";
        }

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cascade cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b =>
                     b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Class schedule {ScheduleId} cancelled: {Reason}", id, reason);
        return null;
    }

    public async Task<List<RosterEntry>> GetRosterAsync(int classId, CancellationToken ct = default)
    {
        return await db.Bookings
            .Where(b => b.ClassScheduleId == classId &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .Include(b => b.Member)
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntry(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.BookingDate, b.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<List<RosterEntry>> GetWaitlistAsync(int classId, CancellationToken ct = default)
    {
        return await db.Bookings
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .Include(b => b.Member)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new RosterEntry(
                b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
                b.BookingDate, b.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<List<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        var schedules = await db.ClassSchedules
            .Where(cs => cs.StartTime > now &&
                        cs.StartTime <= weekFromNow &&
                        cs.Status == ClassScheduleStatus.Scheduled &&
                        cs.CurrentEnrollment < cs.Capacity)
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync(ct);

        return schedules.Select(ToResponse).ToList();
    }

    internal static ClassScheduleResponse ToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status.ToString(), cs.CancellationReason);
}
