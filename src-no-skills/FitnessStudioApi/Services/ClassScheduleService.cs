using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class ClassScheduleService : IClassScheduleService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ClassScheduleDto>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId,
        bool? available, int page, int pageSize)
    {
        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);

        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);

        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);

        if (available == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ClassScheduleDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ClassScheduleDto> GetByIdAsync(int id)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return MapToDto(cs);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new KeyNotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor.");

        var duration = dto.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = dto.Capacity ?? classType.DefaultCapacity;
        var endTime = dto.StartTime.AddMinutes(duration);

        // Check instructor schedule conflict
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < endTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at this time.", 409, "Scheduling Conflict");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Capacity = capacity,
            Room = dto.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Scheduled class '{ClassType}' (ID {Id}) at {StartTime} in {Room}",
            classType.Name, schedule.Id, schedule.StartTime, schedule.Room);

        return await GetByIdAsync(schedule.Id);
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Cannot update a cancelled class.");

        if (!await _db.ClassTypes.AnyAsync(ct => ct.Id == dto.ClassTypeId))
            throw new KeyNotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        if (!await _db.Instructors.AnyAsync(i => i.Id == dto.InstructorId))
            throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        // Check instructor conflict (exclude this schedule)
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at this time.", 409, "Scheduling Conflict");

        schedule.ClassTypeId = dto.ClassTypeId;
        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated class schedule (ID {Id})", schedule.Id);
        return await GetByIdAsync(schedule.Id);
    }

    public async Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.Bookings)
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.CancellationReason;

        // Cancel all bookings for this class
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Cancelled class (ID {Id}), all bookings cancelled", schedule.Id);
        return MapToDto(schedule);
    }

    public async Task<List<ClassRosterItemDto>> GetRosterAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterItemDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Status = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<ClassRosterItemDto>> GetWaitlistAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassRosterItemDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Status = b.Status,
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<ClassScheduleDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var schedules = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime >= now &&
                         cs.StartTime <= nextWeek &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync();

        return schedules.Select(MapToDto).ToList();
    }

    private static ClassScheduleDto MapToDto(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType?.Name ?? "",
        InstructorId = cs.InstructorId,
        InstructorName = cs.Instructor != null ? $"{cs.Instructor.FirstName} {cs.Instructor.LastName}" : "",
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
