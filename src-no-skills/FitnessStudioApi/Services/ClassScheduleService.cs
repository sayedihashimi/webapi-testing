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

        return new PagedResult<ClassScheduleDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
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
            throw new BusinessRuleException("Cannot assign an inactive instructor to a class.");

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict at the specified time.", 409);

        var schedule = new ClassSchedule
        {
            ClassTypeId = dto.ClassTypeId,
            InstructorId = dto.InstructorId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity > 0 ? dto.Capacity : classType.DefaultCapacity,
            Room = dto.Room
        };

        _db.ClassSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Scheduled class {ClassType} with instructor {Instructor} at {StartTime}",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", schedule.StartTime);

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

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new KeyNotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        if (dto.EndTime <= dto.StartTime)
            throw new BusinessRuleException("End time must be after start time.");

        // Check instructor schedule conflicts (excluding current class)
        var hasConflict = await _db.ClassSchedules.AnyAsync(cs =>
            cs.Id != id &&
            cs.InstructorId == dto.InstructorId &&
            cs.Status != ClassScheduleStatus.Cancelled &&
            cs.StartTime < dto.EndTime &&
            cs.EndTime > dto.StartTime);

        if (hasConflict)
            throw new BusinessRuleException("Instructor has a schedule conflict at the specified time.", 409);

        schedule.ClassTypeId = dto.ClassTypeId;
        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Include(cs => cs.Bookings)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.CancellationReason;
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

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled class {ClassId} with reason: {Reason}", id, dto.CancellationReason);
        return MapToDto(schedule);
    }

    public async Task<List<BookingDto>> GetRosterAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var bookings = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(MemberService.MapBookingToDto).ToList();
    }

    public async Task<List<BookingDto>> GetWaitlistAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new KeyNotFoundException($"Class schedule with ID {id} not found.");

        var bookings = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync();

        return bookings.Select(MemberService.MapBookingToDto).ToList();
    }

    public async Task<List<ClassScheduleDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        var classes = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                         cs.StartTime > now &&
                         cs.StartTime <= weekFromNow &&
                         cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync();

        return classes.Select(MapToDto).ToList();
    }

    internal static ClassScheduleDto MapToDto(ClassSchedule cs) => new()
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
        Status = cs.Status.ToString(),
        CancellationReason = cs.CancellationReason,
        IsPremium = cs.ClassType.IsPremium
    };
}
