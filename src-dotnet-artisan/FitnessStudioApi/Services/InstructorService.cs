using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorDto>> GetAllAsync(string? specialization, bool? isActive)
    {
        var query = db.Instructors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var s = specialization.ToLower();
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(s));
        }

        return await query.OrderBy(i => i.LastName).Select(i => ToDto(i)).ToListAsync();
    }

    public async Task<InstructorDto?> GetByIdAsync(int id)
    {
        var instructor = await db.Instructors.FindAsync(id);
        return instructor is null ? null : ToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorDto dto)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == dto.Email))
            throw new ValidationException("An instructor with this email already exists.");

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
        db.Instructors.Add(instructor);
        await db.SaveChangesAsync();
        logger.LogInformation("Created instructor {Email} (Id={InstructorId})", instructor.Email, instructor.Id);
        return ToDto(instructor);
    }

    public async Task<InstructorDto?> UpdateAsync(int id, UpdateInstructorDto dto)
    {
        var instructor = await db.Instructors.FindAsync(id);
        if (instructor is null) return null;

        if (await db.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new ValidationException("An instructor with this email already exists.");

        instructor.FirstName = dto.FirstName;
        instructor.LastName = dto.LastName;
        instructor.Email = dto.Email;
        instructor.Phone = dto.Phone;
        instructor.Bio = dto.Bio;
        instructor.Specializations = dto.Specializations;
        instructor.IsActive = dto.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated instructor {InstructorId}", id);
        return ToDto(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue) query = query.Where(cs => cs.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(cs => cs.StartTime <= to.Value);

        return await query.OrderBy(cs => cs.StartTime)
            .Select(cs => ClassScheduleService.ToDto(cs))
            .ToListAsync();
    }

    private static InstructorDto ToDto(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate,
        i.IsActive, i.CreatedAt, i.UpdatedAt);
}
