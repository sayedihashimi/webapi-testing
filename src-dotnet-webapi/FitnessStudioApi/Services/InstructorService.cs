using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<PaginatedResponse<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Instructors.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.Contains(specialization));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.LastName).ThenBy(i => i.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);

        return PaginatedResponse<InstructorResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await db.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == request.Email, ct))
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

        logger.LogInformation("Created instructor {InstructorEmail} with ID {InstructorId}", instructor.Email, instructor.Id);
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null) return null;

        if (await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct))
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated instructor {InstructorId}", instructor.Id);
        return MapToResponse(instructor);
    }

    public async Task<List<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        if (!await db.Instructors.AnyAsync(i => i.Id == instructorId, ct))
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = db.ClassSchedules.AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) >= from.Value);

        if (to.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) <= to.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync(ct);

        return schedules.Select(cs => new ClassScheduleResponse
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
            CreatedAt = cs.CreatedAt,
            UpdatedAt = cs.UpdatedAt
        }).ToList();
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
}
