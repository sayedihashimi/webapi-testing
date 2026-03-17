using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class ClassScheduleService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<ClassScheduleService> _logger;

    public ClassScheduleService(FitnessDbContext db, ILogger<ClassScheduleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ClassScheduleDto>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? hasAvailability,
        int page = 1, int pageSize = 10)
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
        if (hasAvailability == true)
            query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => MapToDto(cs))
            .ToListAsync();

        return new PaginatedResponse<ClassScheduleDto>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ClassScheduleDto?> GetByIdAsync(int id)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cs == null ? null : MapToDto(cs);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new BusinessRuleException($"Class type with ID {dto.ClassTypeId} not found.", 404);

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new BusinessRuleException($"Instructor with ID {dto.InstructorId} not found.", 404);

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor.");

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        await CheckInstructorConflict(dto.InstructorId, dto.StartTime, dto.EndTime, excludeId: null);

        var cs = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity,
            Room = dto.Room
        };

        _db.ClassSchedules.Add(cs);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created class schedule {ScheduleId} for {ClassType} with {Instructor}",
            cs.Id, classType.Name, $"{instructor.FirstName} {instructor.LastName}");

        return (await GetByIdAsync(cs.Id))!;
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessRuleException($"Class schedule with ID {id} not found.", 404);

        if (cs.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Can only update scheduled classes.");

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        await CheckInstructorConflict(dto.InstructorId, dto.StartTime, dto.EndTime, excludeId: id);

        cs.ClassTypeId = dto.ClassTypeId;
        cs.InstructorId = dto.InstructorId;
        cs.StartTime = dto.StartTime;
        cs.EndTime = dto.EndTime;
        cs.Capacity = dto.Capacity;
        cs.Room = dto.Room;
        cs.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated class schedule {ScheduleId}", id);
        return (await GetByIdAsync(id))!;
    }

    public async Task<ClassScheduleDto> CancelClassAsync(int id, CancelClassDto dto)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessRuleException($"Class schedule with ID {id} not found.", 404);

        if (cs.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        if (cs.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a completed class.");

        cs.Status = ClassScheduleStatus.Cancelled;
        cs.CancellationReason = dto.Reason;
        cs.UpdatedAt = DateTime.UtcNow;

        // Cancel all active bookings
        foreach (var booking in cs.Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        cs.CurrentEnrollment = 0;
        cs.WaitlistCount = 0;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled class schedule {ScheduleId} with reason: {Reason}", id, dto.Reason);
        return MapToDto(cs);
    }

    public async Task<List<ClassRosterEntryDto>> GetRosterAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new BusinessRuleException($"Class schedule with ID {id} not found.", 404);

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended || b.Status == BookingStatus.NoShow))
            .OrderBy(b => b.BookingDate)
            .Select(b => new ClassRosterEntryDto
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

    public async Task<List<ClassWaitlistEntryDto>> GetWaitlistAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new BusinessRuleException($"Class schedule with ID {id} not found.", 404);

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new ClassWaitlistEntryDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FirstName + " " + b.Member.LastName,
                WaitlistPosition = b.WaitlistPosition,
                BookingDate = b.BookingDate
            })
            .ToListAsync();
    }

    public async Task<List<ClassScheduleDto>> GetAvailableAsync()
    {
        return await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > DateTime.UtcNow &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapToDto(cs))
            .ToListAsync();
    }

    private async Task CheckInstructorConflict(int instructorId, DateTime start, DateTime end, int? excludeId)
    {
        var conflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == instructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            (excludeId == null || cs.Id != excludeId) &&
            cs.StartTime < end && cs.EndTime > start);

        if (conflict)
            throw new BusinessRuleException("Instructor has a scheduling conflict during this time period.", 409);
    }

    public static ClassScheduleDto MapToDto(ClassSchedule cs) => new()
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
        IsPremium = cs.ClassType.IsPremium,
        CreatedAt = cs.CreatedAt,
        UpdatedAt = cs.UpdatedAt
    };
}
