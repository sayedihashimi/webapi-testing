using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class InstructorService
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

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<InstructorDto?> GetByIdAsync(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        return instructor == null ? null : MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409);

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
        _logger.LogInformation("Created instructor {InstructorEmail} with ID {InstructorId}", instructor.Email, instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _db.Instructors.FindAsync(id)
            ?? throw new BusinessRuleException($"Instructor with ID {id} not found.", 404);

        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.", 409);

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated instructor {InstructorId}", id);
        return MapToDto(instructor);
    }

    public async Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new BusinessRuleException($"Instructor with ID {instructorId} not found.", 404);

        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
            query = query.Where(cs => cs.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(cs => cs.StartTime <= to.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => ClassScheduleService.MapToDto(cs))
            .ToListAsync();
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
}
