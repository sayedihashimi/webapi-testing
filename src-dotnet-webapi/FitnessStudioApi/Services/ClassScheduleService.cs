using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(
        DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) <= toDate.Value);

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassStatus.Scheduled);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(MapToResponse).ToList();
        return PaginatedResponse<ClassScheduleResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cs = await db.ClassSchedules
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return cs is null ? null : MapToResponse(cs);
    }

    public async Task<ClassScheduleResponse> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([request.ClassTypeId], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new ArgumentException("Cannot schedule a class with an inactive instructor.");

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict during this time.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = request.ClassTypeId,
            InstructorId = request.InstructorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            CurrentEnrollment = 0,
            WaitlistCount = 0,
            Room = request.Room,
            Status = ClassStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClassSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        // Reload with includes
        var created = await db.ClassSchedules
            .AsNoTracking()
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstAsync(c => c.Id == schedule.Id, ct);

        logger.LogInformation("Created class schedule {ScheduleId}: {ClassName}", schedule.Id, classType.Name);
        return MapToResponse(created);
    }

    public async Task<ClassScheduleResponse> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassStatus.Cancelled)
            throw new ArgumentException("Cannot update a cancelled class.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        // Check instructor conflicts (exclude current)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a schedule conflict during this time.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        if (request.Capacity < schedule.CurrentEnrollment)
            throw new ArgumentException($"Cannot reduce capacity below current enrollment ({schedule.CurrentEnrollment}).");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        // Reload includes
        await db.Entry(schedule).Reference(s => s.Instructor).LoadAsync(ct);

        logger.LogInformation("Updated class schedule {ScheduleId}", schedule.Id);
        return MapToResponse(schedule);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassStatus.Cancelled)
            throw new ArgumentException("Class is already cancelled.");

        if (schedule.Status == ClassStatus.Completed)
            throw new ArgumentException("Cannot cancel a completed class.");

        schedule.Status = ClassStatus.Cancelled;
        schedule.CancellationReason = request.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled class schedule {ScheduleId} with reason: {Reason}", id, request.Reason);
        return MapToResponse(schedule);
    }

    public async Task<IReadOnlyList<ClassRosterEntryResponse>> GetRosterAsync(int id, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var bookings = await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .ToListAsync(ct);

        return bookings.Select(b => new ClassRosterEntryResponse(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.Member.Email, b.Status, b.BookingDate, b.CheckInTime)).ToList();
    }

    public async Task<IReadOnlyList<WaitlistEntryResponse>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists)
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var waitlisted = await db.Bookings
            .AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync(ct);

        return waitlisted.Select(b => new WaitlistEntryResponse(
            b.Id, b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.WaitlistPosition, b.BookingDate)).ToList();
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        var schedules = await db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassStatus.Scheduled &&
                cs.StartTime >= now &&
                cs.StartTime <= weekFromNow &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync(ct);

        return schedules.Select(MapToResponse).ToList();
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) =>
        new(cs.Id, cs.ClassTypeId, cs.ClassType.Name,
            cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
            cs.Room, cs.Status, cs.CancellationReason, cs.CreatedAt, cs.UpdatedAt);
}
