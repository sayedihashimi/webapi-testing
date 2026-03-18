using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<PagedResponse<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Instructors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(specialization.ToLower()));

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);

        return PagedResponse<InstructorResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        var emailExists = await db.Instructors.AnyAsync(i => i.Email == request.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");

        var instructor = new Instructor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Bio = request.Bio,
            Specializations = request.Specializations,
            HireDate = request.HireDate
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created instructor {InstructorId} '{Name}'", instructor.Id, $"{instructor.FirstName} {instructor.LastName}");
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null) return null;

        var emailConflict = await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated instructor {InstructorId}", id);
        return MapToResponse(instructor);
    }

    public async Task<List<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct)
    {
        var exists = await db.Instructors.AnyAsync(i => i.Id == instructorId, ct);
        if (!exists) throw new KeyNotFoundException($"Instructor {instructorId} not found.");

        var query = db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) <= toDate.Value);

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => MapScheduleToResponse(cs))
            .ToListAsync(ct);
    }

    private static InstructorResponse MapToResponse(Instructor i) => new()
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

    private static ClassScheduleResponse MapScheduleToResponse(ClassSchedule cs) => new()
    {
        Id = cs.Id,
        ClassTypeId = cs.ClassTypeId,
        ClassTypeName = cs.ClassType?.Name ?? string.Empty,
        InstructorId = cs.InstructorId,
        InstructorName = cs.Instructor is not null ? $"{cs.Instructor.FirstName} {cs.Instructor.LastName}" : string.Empty,
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
