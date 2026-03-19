using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PagedResult<ClassScheduleResponse>> GetAllAsync(
        DateOnly? date, int? classTypeId, int? instructorId, bool? hasAvailability,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (date.HasValue)
        {
            var startOfDay = date.Value.ToDateTime(TimeOnly.MinValue);
            var endOfDay = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(cs => cs.StartTime >= startOfDay && cs.StartTime <= endOfDay);
        }

        if (classTypeId.HasValue)
        {
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(cs => cs.InstructorId == instructorId.Value);
        }

        if (hasAvailability == true)
        {
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);
        }

        var totalCount = await query.CountAsync(ct);

        var schedules = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ClassScheduleResponse>(
            schedules.Select(MapToResponse).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        return schedule is null ? null : MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct = default)
    {
        _ = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new InvalidOperationException("Class type not found.");

        _ = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new InvalidOperationException("Instructor not found.");

        // Check instructor conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
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

        logger.LogInformation("Created class schedule {Id} for class type {ClassTypeId}", schedule.Id, request.ClassTypeId);

        return await GetByIdAsync(schedule.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created schedule.");
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null)
        {
            return null;
        }

        // Check instructor conflicts (excluding current schedule)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
        {
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");
        }

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated class schedule {Id}", id);

        return await GetByIdAsync(schedule.Id, ct);
    }

    public async Task<ClassScheduleResponse> CancelClassAsync(int id, string reason, CancellationToken ct = default)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException("Class schedule not found.");

        if (schedule.Status is ClassScheduleStatus.Completed or ClassScheduleStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel a class that is {schedule.Status}.");
        }

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
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

        logger.LogInformation("Cancelled class schedule {Id} with reason: {Reason}", id, reason);

        return MapToResponse(schedule);
    }

    public async Task<List<ClassRosterEntry>> GetRosterAsync(int classId, CancellationToken ct = default)
    {
        var bookings = await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId &&
                        b.Status != BookingStatus.Waitlisted &&
                        b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.BookingDate)
            .ToListAsync(ct);

        return bookings.Select(b => new ClassRosterEntry(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.Status, b.BookingDate, b.CheckInTime)).ToList();
    }

    public async Task<List<ClassWaitlistEntry>> GetWaitlistAsync(int classId, CancellationToken ct = default)
    {
        var bookings = await db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == classId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync(ct);

        return bookings.Select(b => new ClassWaitlistEntry(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.WaitlistPosition, b.BookingDate)).ToList();
    }

    public async Task<List<ClassScheduleResponse>> GetAvailableClassesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var schedules = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime >= now &&
                         cs.StartTime <= nextWeek &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync(ct);

        return schedules.Select(MapToResponse).ToList();
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status, cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
