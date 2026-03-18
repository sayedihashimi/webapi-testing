using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
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

    public async Task<PagedResult<ClassScheduleDto>> GetAllAsync(
        DateTime? from, DateTime? to, int? classTypeId, int? instructorId,
        bool? hasAvailability, int page, int pageSize)
    {
        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .AsQueryable();

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);
        if (classTypeId.HasValue) query = query.Where(cs => cs.ClassTypeId == classTypeId.Value);
        if (instructorId.HasValue) query = query.Where(cs => cs.InstructorId == instructorId.Value);
        if (hasAvailability == true) query = query.Where(cs => cs.CurrentEnrollment < cs.Capacity && cs.Status == ClassScheduleStatus.Scheduled);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(cs => cs.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(ToDto).ToList();
        return new PagedResult<ClassScheduleDto>(dtos, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<ClassScheduleDetailDto?> GetByIdAsync(int id)
    {
        var cs = await _db.ClassSchedules
            .Include(c => c.ClassType)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cs is null) return null;

        var available = Math.Max(0, cs.Capacity - cs.CurrentEnrollment);
        return new ClassScheduleDetailDto(
            cs.Id, cs.ClassTypeId, cs.ClassType.Name, cs.InstructorId,
            $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
            cs.WaitlistCount, available, cs.Room, cs.Status,
            cs.CancellationReason, cs.CreatedAt, cs.UpdatedAt);
    }

    public async Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(dto.ClassTypeId)
            ?? throw new NotFoundException($"Class type with ID {dto.ClassTypeId} not found.");

        var instructor = await _db.Instructors.FindAsync(dto.InstructorId)
            ?? throw new NotFoundException($"Instructor with ID {dto.InstructorId} not found.");

        if (!instructor.IsActive)
            throw new BusinessRuleException("Cannot assign an inactive instructor.");

        var duration = dto.DurationMinutes ?? classType.DefaultDurationMinutes;
        var capacity = dto.Capacity ?? classType.DefaultCapacity;
        var endTime = dto.StartTime.AddMinutes(duration);

        // Check instructor schedule conflict
        var conflict = await _db.ClassSchedules
            .AnyAsync(cs => cs.InstructorId == dto.InstructorId &&
                cs.Status != ClassScheduleStatus.Cancelled &&
                cs.StartTime < endTime && cs.EndTime > dto.StartTime);

        if (conflict)
            throw new ConflictException("Instructor has a schedule conflict at this time.");

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

        await _db.Entry(schedule).Reference(s => s.ClassType).LoadAsync();
        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync();

        _logger.LogInformation("Class scheduled: {ClassType} with {Instructor} at {StartTime}",
            classType.Name, $"{instructor.FirstName} {instructor.LastName}", dto.StartTime);

        return ToDto(schedule);
    }

    public async Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto)
    {
        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new NotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Cannot update a cancelled class.");

        // Check instructor conflict if changed
        if (dto.InstructorId != schedule.InstructorId || dto.StartTime != schedule.StartTime || dto.EndTime != schedule.EndTime)
        {
            var conflict = await _db.ClassSchedules
                .AnyAsync(cs => cs.InstructorId == dto.InstructorId && cs.Id != id &&
                    cs.Status != ClassScheduleStatus.Cancelled &&
                    cs.StartTime < dto.EndTime && cs.EndTime > dto.StartTime);
            if (conflict)
                throw new ConflictException("Instructor has a schedule conflict at this time.");
        }

        schedule.ClassTypeId = dto.ClassTypeId;
        schedule.InstructorId = dto.InstructorId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.Capacity = dto.Capacity;
        schedule.Room = dto.Room;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(schedule).Reference(s => s.ClassType).LoadAsync();
        await _db.Entry(schedule).Reference(s => s.Instructor).LoadAsync();

        return ToDto(schedule);
    }

    public async Task CancelAsync(int id, CancelClassDto dto)
    {
        var schedule = await _db.ClassSchedules.FindAsync(id)
            ?? throw new NotFoundException($"Class schedule with ID {id} not found.");

        if (schedule.Status == ClassScheduleStatus.Cancelled)
            throw new BusinessRuleException("Class is already cancelled.");

        schedule.Status = ClassScheduleStatus.Cancelled;
        schedule.CancellationReason = dto.Reason;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cancel all bookings
        var bookings = await _db.Bookings
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted))
            .ToListAsync();

        foreach (var booking in bookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Class cancelled by studio";
            booking.UpdatedAt = DateTime.UtcNow;
        }

        schedule.CurrentEnrollment = 0;
        schedule.WaitlistCount = 0;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Class {ClassId} cancelled. {Count} bookings auto-cancelled.", id, bookings.Count);
    }

    public async Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new NotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .OrderBy(b => b.BookingDate)
            .Select(b => new RosterEntryDto(b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}", b.Member.Email, b.Status, b.BookingDate, b.CheckInTime))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id)
    {
        if (!await _db.ClassSchedules.AnyAsync(cs => cs.Id == id))
            throw new NotFoundException($"Class schedule with ID {id} not found.");

        return await _db.Bookings
            .Include(b => b.Member)
            .Where(b => b.ClassScheduleId == id && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .Select(b => new WaitlistEntryDto(b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}", b.WaitlistPosition, b.BookingDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClassScheduleDto>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var weekFromNow = now.AddDays(7);

        var schedules = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.Status == ClassScheduleStatus.Scheduled &&
                cs.StartTime > now && cs.StartTime <= weekFromNow &&
                cs.CurrentEnrollment < cs.Capacity)
            .OrderBy(cs => cs.StartTime)
            .ToListAsync();

        return schedules.Select(ToDto).ToList();
    }

    internal static ClassScheduleDto ToDto(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name, cs.InstructorId,
        $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment,
        cs.WaitlistCount, cs.Room, cs.Status, cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
