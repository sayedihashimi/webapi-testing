using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
{
    public async Task<PagedResponse<ClassScheduleResponse>> GetAllAsync(DateOnly? date, int? classTypeId, int? instructorId, bool? available, int page, int pageSize, CancellationToken ct)
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
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled && cs.StartTime > DateTime.UtcNow);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);

        return PagedResponse<ClassScheduleResponse>.Create(items, page, pageSize, totalCount);
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
            throw new ArgumentException("Cannot schedule a class with an inactive instructor.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflict
        var conflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (conflict)
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");

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

        logger.LogInformation("Created class schedule {ScheduleId} for {ClassType} with {Instructor}",
            schedule.Id, classType.Name, $"{instructor.FirstName} {instructor.LastName}");

        return await GetByIdAsync(schedule.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created schedule.");
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        if (schedule is null) return null;

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled class.");

        var instructor = await db.Instructors.FindAsync([request.InstructorId], ct)
            ?? throw new KeyNotFoundException($"Instructor {request.InstructorId} not found.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflict (excluding this schedule)
        var conflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (conflict)
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");

        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated class schedule {ScheduleId}", id);

        return await GetByIdAsync(id, ct);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.Reason ?? "Cancelled by studio";

        // Cancel all bookings
        foreach (var booking in schedule.Bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled class schedule {ScheduleId} and all bookings", id);
        return MapToResponse(schedule);
    }

    public async Task<List<ClassRosterEntry>> GetRosterAsync(int id, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists) throw new KeyNotFoundException($"Class schedule {id} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status != BookingStatus.Waitlisted && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterEntry
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
                Status = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync(ct);
    }

    public async Task<List<WaitlistEntry>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        var exists = await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct);
        if (!exists) throw new KeyNotFoundException($"Class schedule {id} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntry
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
                WaitlistPosition = b.WaitlistPosition,
                BookingDate = b.BookingDate
            })
            .ToListAsync(ct);
    }

    public async Task<List<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var weekAhead = now.AddDays(7);

        return await db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.StartTime <= weekAhead &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToResponse(cs))
            .ToListAsync(ct);
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType?.Name ?? string.Empty,
        InstructorId = cs.InstructorId,
        InstructorName = cs.Instructor is not null ? $"{cs.Instructor.FirstName} {cs.Instructor.LastName}" : string.Empty,
        StartTime = cs.StartTime,
        EndTime = cs.EndTime,
        Capacity = cs.Capacity,
        CurrentEnrollment = cs.CurrentEnrollment,
        WaitlistCount = cs.WaitlistCount,
        Room = cs.Room,
        Status = cs.Status,
        CancellationReason = cs.CancellationReason,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
