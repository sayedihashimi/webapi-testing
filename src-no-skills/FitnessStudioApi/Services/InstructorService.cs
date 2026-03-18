using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class InstructorService : IInstructorService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<InstructorService> _logger;

    public InstructorService(FitnessDbContext db, ILogger<InstructorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<InstructorDto>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = _db.Instructors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        return await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<InstructorDto> GetByIdAsync(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");
        return MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Conflict");

        var instructor = new Instructor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Bio = dto.Bio,
            Specializations = dto.Specializations,
            HireDate = dto.HireDate
        };

        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created instructor '{FirstName} {LastName}' (ID {Id})", instructor.FirstName, instructor.LastName, instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _db.Instructors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");

        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409, "Conflict");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated instructor (ID {Id})", instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync();
        return schedules.Select(MapScheduleToDto).ToList();
    }

    private static InstructorDto MapToDto(Instructor i) => new()
    {
        Id = i.Id,
        FirstName = i.FirstName,
        LastName = i.LastName,
        Email = i.Email,
        Phone = i.Phone,
        Bio = i.Bio,
        Specializations = i.Specializations,
        HireDate = i.HireDate,
        IsActive = i.IsActive,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };

    private static ClassScheduleDto MapScheduleToDto(ClassSchedule cs) => new()
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
