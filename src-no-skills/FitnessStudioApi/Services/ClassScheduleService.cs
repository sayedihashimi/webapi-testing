using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedResult<ClassScheduleResponseDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId,
        bool? hasAvailability, PaginationParams pagination)
    {
        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);
        if (classTypeId.HasValue)
            query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue)
            query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<ClassScheduleResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ClassScheduleResponseDto?> GetByIdAsync(int id)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
        return cs is null ? null : MapToDto(cs);
    }

    public async Task<ClassScheduleResponseDto> CreateAsync(ClassScheduleCreateDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new BusinessRuleException("Class type not found.", 404);

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new BusinessRuleException("Instructor not found.", 404);

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor.");

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at the specified time.");

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity ?? classType.DefaultCapacity,
            Room = dto.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Scheduled class {ClassTypeName} with instructor {InstructorName} at {StartTime}",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", schedule.StartTime);

        // Reload with includes
        var result = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstAsync(c => c.Id == schedule.Id);
        return MapToDto(result);
    }

    public async Task<ClassScheduleResponseDto?> UpdateAsync(int id, ClassScheduleUpdateDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (schedule is null) return null;

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Cannot update a cancelled class.");

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new BusinessRuleException("Instructor not found.", 404);

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor conflicts (excluding this schedule)
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict at the specified time.");

        schedule.ClassTypeId = dto.ClassTypeId;
        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        if (dto.Capacity.HasValue)
            schedule.Capacity = dto.Capacity.Value;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated class schedule {ScheduleId}", id);

        // Reload
        schedule = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstAsync(c => c.Id == id);
        return MapToDto(schedule);
    }

    public async Task<ClassScheduleResponseDto> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessRuleException("Class schedule not found.", 404);

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        if (schedule.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a completed class.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in schedule.Bookings.Where(b =>
            b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.WaitlistPosition = null;
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled class schedule {ScheduleId}. All bookings cancelled.", id);
        return MapToDto(schedule);
    }

    public async Task<List<RosterEntryDto>> GetRosterAsync(int id)
    {
        var schedule = await _db.ClassSchedules.FindAsync(id)
            ?? throw new BusinessRuleException("Class schedule not found.", 404);

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                Status = b.Status.ToString(),
                BookingDate = b.BookingDate,
                CheckInTime = b.CheckInTime
            })
            .ToListAsync();
    }

    public async Task<List<WaitlistEntryDto>> GetWaitlistAsync(int id)
    {
        var schedule = await _db.ClassSchedules.FindAsync(id)
            ?? throw new BusinessRuleException("Class schedule not found.", 404);

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                WaitlistPosition = b.WaitlistPosition ?? 0,
                BookingDate = b.BookingDate
            })
            .ToListAsync();
    }

    public async Task<List<ClassScheduleResponseDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekOut = now.AddDays(7);

        return await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.StartTime <= weekOut &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToDto(cs))
            .ToListAsync();
    }

    internal static ClassScheduleResponseDto MapToDto(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType?.Name ?? string.Empty,
        InstructorId = cs.InstructorId,
        InstructorName = cs.Instructor != null ? $"{cs.Instructor.FirstName} {cs.Instructor.LastName}" : string.Empty,
        StartTime = cs.StartTime,
        EndTime = cs.EndTime,
        Capacity = cs.Capacity,
        CurrentEnrollment = cs.CurrentEnrollment,
        WaitlistCount = cs.WaitlistCount,
        Room = cs.Room,
        Status = cs.Status.ToString(),
        CancellationReason = cs.CancellationReason,
        IsPremium = cs.ClassType?.IsPremium ?? false,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
