using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<InstructorResponseDto>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = _db.Instructors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<InstructorResponseDto?> GetByIdAsync(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        return instructor is null ? null : MapToDto(instructor);
    }

    public async Task<InstructorResponseDto> CreateAsync(InstructorCreateDto dto)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.");

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
        _logger.LogInformation("Created instructor {InstructorName} with ID {InstructorId}", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToDto(instructor);
    }

    public async Task<InstructorResponseDto?> UpdateAsync(int id, InstructorUpdateDto dto)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor is null) return null;

        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new BusinessRuleException($"An instructor with email '{dto.Email}' already exists.");

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

    public async Task<List<ClassScheduleResponseDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new BusinessRuleException("Instructor not found.", 404);

        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => cs.StartTime >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cs => cs.StartTime <= toDate.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => ClassScheduleService.MapToDto(cs))
            .ToListAsync();
    }

    private static InstructorResponseDto MapToDto(Instructor i) => new()
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
