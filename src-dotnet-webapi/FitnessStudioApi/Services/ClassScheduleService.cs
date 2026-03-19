using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(StudioDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateOnly? date, int? classTypeId, int? instructorId, bool? available, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) == date.Value);

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (available == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return PaginatedResponse<ClassScheduleResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await db.ClassSchedules.AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return cs is null ? null : MapToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor {request.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new InvalidOperationException("Cannot assign inactive instructor to a class.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflict
        var hasConflict = await db.ClassSchedules.AnyAsync(
            cs => cs.InstructorId == request.InstructorId
                && cs.Status != ClassScheduleStatus.Cancelled
                && cs.StartTime < request.EndTime
                && cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            Room = request.Room,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        var result = await db.ClassSchedules.AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstAsync(c => c.Id == schedule.Id, ct);

        logger.LogInformation("Created class schedule {ScheduleId}: {ClassName}", schedule.Id, classType.Name);
        return MapToResponse(result);
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule {id} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Can only update scheduled classes.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor conflict (excluding this schedule)
        var hasConflict = await db.ClassSchedules.AnyAsync(
            cs => cs.InstructorId == request.InstructorId
                && cs.Id != id
                && cs.Status != ClassScheduleStatus.Cancelled
                && cs.StartTime < request.EndTime
                && cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict at the requested time.");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated class schedule {ScheduleId}", id);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule {id} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Can only cancel scheduled classes.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = $"Class cancelled: {request.Reason}";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled class schedule {ScheduleId}: {Reason}", id, request.Reason);
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<ClassRosterResponse>> GetRosterAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new KeyNotFoundException($"Class schedule {id} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterResponse(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.Status.ToString(), b.BookingDate, b.CheckInTime))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassWaitlistResponse>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new KeyNotFoundException($"Class schedule {id} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassWaitlistResponse(
                b.Id, b.MemberId,
                $"{b.Member.FirstName} {b.Member.LastName}",
                b.WaitlistPosition, b.BookingDate))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekAhead = now.AddDays(7);

        return await db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled
                && cs.StartTime > now
                && cs.StartTime <= weekAhead
                && cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) =>
        new(cs.Id, cs.ClassTypeId, cs.ClassType.Name, cs.InstructorId,
            $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
            cs.WaitlistCount, Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
            cs.Room, cs.Status.ToString(), cs.CancellationReason,
            cs.CreatedAt, cs.UpdatedAt);
}
