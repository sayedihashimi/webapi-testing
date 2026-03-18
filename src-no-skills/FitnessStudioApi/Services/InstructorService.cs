using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class InstructorService : IInstructorService
{
    private readonly FitnessDbContext _db;

    public InstructorService(FitnessDbContext db) => _db = db;

    public async Task<IReadOnlyList<InstructorDto>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = _db.Instructors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        return await query.OrderBy(i => i.LastName).Select(i => ToDto(i)).ToListAsync();
    }

    public async Task<InstructorDto?> GetByIdAsync(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        return instructor is null ? null : ToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new ConflictException($"An instructor with email '{dto.Email}' already exists.");

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
        return ToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await _db.Instructors.FindAsync(id)
            ?? throw new NotFoundException($"Instructor with ID {id} not found.");

        if (await _db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new ConflictException($"An instructor with email '{dto.Email}' already exists.");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        if (!await _db.Instructors.AnyAsync(i => i.Id == instructorId))
            throw new NotFoundException($"Instructor with ID {instructorId} not found.");

        var query = _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync();
        return schedules.Select(ClassScheduleService.ToDto).ToList();
    }

    private static InstructorDto ToDto(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);
}
