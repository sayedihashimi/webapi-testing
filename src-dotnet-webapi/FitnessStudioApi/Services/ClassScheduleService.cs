using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger) : IClassScheduleService
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
            .ToListAsync(ct);

        return PaginatedResponse<ClassScheduleResponse>.Create(
            items.Select(MapToResponse).ToList(), page, pageSize, totalCount);
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
        if (!await db.ClassTypes.AnyAsync(c => c.Id == request.ClassTypeId, ct))
            throw new KeyNotFoundException($"Class type with ID {request.ClassTypeId} not found.");

        if (!await db.Instructors.AnyAsync(i => i.Id == request.InstructorId, ct))
            throw new KeyNotFoundException($"Instructor with ID {request.InstructorId} not found.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
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

        logger.LogInformation("Created class schedule {ScheduleId} for class type {ClassTypeId}", schedule.Id, schedule.ClassTypeId);

        return await GetByIdAsync(schedule.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created schedule.");
    }

    public async Task<ClassScheduleResponse?> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules.FindAsync([id], ct);
        if (schedule is null) return null;

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled class.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be after start time.");

        // Check instructor conflicts (excluding self)
        var hasConflict = await db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == request.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < request.EndTime &&
            cs.EndTime > request.StartTime, ct);

        if (hasConflict)
            throw new InvalidOperationException("Instructor has a scheduling conflict during this time.");

        schedule.ClassTypeId = request.ClassTypeId;
        schedule.InstructorId = request.InstructorId;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.Capacity = request.Capacity;
        schedule.Room = request.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated class schedule {ScheduleId}", schedule.Id);

        return await GetByIdAsync(schedule.Id, ct);
    }

    public async Task<ClassScheduleResponse> CancelAsync(int id, CancelClassRequest request, CancellationToken ct)
    {
        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new InvalidOperationException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = request.CancellationReason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all bookings for this class
        var bookings = await db.Bookings
            .Where(b => b.ClassScheduleId == id &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
            .ToListAsync(ct);

        foreach (var booking in bookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = $"Class cancelled: {request.CancellationReason ?? "No reason provided"}";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled class schedule {ScheduleId}, cancelled {BookingCount} bookings", schedule.Id, bookings.Count);
        return MapToResponse(schedule);
    }

    public async Task<List<ClassRosterResponse>> GetRosterAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var bookings = await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .ToListAsync(ct);

        return bookings.Select(b => new ClassRosterResponse
        {
            MemberId = b.MemberId,
            MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
            BookingStatus = b.Status,
            BookingDate = b.BookingDate,
            CheckInTime = b.CheckInTime
        }).ToList();
    }

    public async Task<List<ClassRosterResponse>> GetWaitlistAsync(int id, CancellationToken ct)
    {
        if (!await db.ClassSchedules.AnyAsync(cs => cs.Id == id, ct))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var bookings = await db.Bookings.AsNoTracking()
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync(ct);

        return bookings.Select(b => new ClassRosterResponse
        {
            MemberId = b.MemberId,
            MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
            BookingStatus = b.Status,
            BookingDate = b.BookingDate,
            CheckInTime = b.CheckInTime
        }).ToList();
    }

    public async Task<List<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var schedules = await db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.StartTime <= nextWeek &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync(ct);

        return schedules.Select(MapToResponse).ToList();
    }

    private static ClassScheduleResponse MapToResponse(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType.Name,
        InstructorId = cs.InstructorId,
        InstructorName = $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
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
