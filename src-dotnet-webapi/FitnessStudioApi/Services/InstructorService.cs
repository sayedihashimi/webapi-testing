using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<PaginatedResponse<InstructorResponse>> GetAllAsync(
        string? specialization, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Instructors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(i => i.Specializations != null && i.Specializations.Contains(specialization));

        if (isActive.HasValue)
            query = query.Where(i => i.IsActive == isActive.Value);

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
        var instructor = await db.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
        var duplicate = await db.Instructors.AnyAsync(i => i.Email == request.Email, ct);
        if (duplicate)
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");

        var instructor = new Instructor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Bio = request.Bio,
            Specializations = request.Specializations,
            HireDate = request.HireDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Instructors.Add(instructor);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created instructor {InstructorId}: {Name}", instructor.Id, $"{instructor.FirstName} {instructor.LastName}");
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Instructor with ID {id} not found.");

        var duplicate = await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct);
        if (duplicate)
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

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(
        int instructorId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct)
    {
        var exists = await db.Instructors.AnyAsync(i => i.Id == instructorId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

        var query = db.ClassSchedules
            .AsNoTracking()
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(cs => DateOnly.FromDateTime(cs.StartTime) <= toDate.Value);

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync(ct);

        return schedules.Select(MapScheduleToResponse).ToList();
    }

    private static InstructorResponse MapToResponse(Instructor i) =>
        new(i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
            i.Bio, i.Specializations, i.HireDate, i.IsActive,
            i.CreatedAt, i.UpdatedAt);

    private static ClassScheduleResponse MapScheduleToResponse(ClassSchedule cs) =>
        new(cs.Id, cs.ClassTypeId, cs.ClassType.Name,
            cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
            cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
            Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
            cs.Room, cs.Status, cs.CancellationReason, cs.CreatedAt, cs.UpdatedAt);
}
