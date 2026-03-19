using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(FitnessDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<List<InstructorResponse>> GetAllAsync(
        string? specialization, bool? isActive, CancellationToken ct = default)
    {
        var query = db.Instructors.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(i => i.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var term = specialization.ToLower();
            query = query.Where(i => i.Specializations != null && i.Specializations.ToLower().Contains(term));
        }

        var instructors = await query.OrderBy(i => i.LastName).ToListAsync(ct);
        return instructors.Select(MapToResponse).ToList();
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default)
    {
        if (await db.Instructors.AnyAsync(i => i.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");
        }

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

        logger.LogInformation("Created instructor {Name} with ID {Id}", $"{instructor.FirstName} {instructor.LastName}", instructor.Id);

        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct = default)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null)
        {
            return null;
        }

        if (await db.Instructors.AnyAsync(i => i.Email == request.Email && i.Id != id, ct))
        {
            throw new InvalidOperationException($"An instructor with email '{request.Email}' already exists.");
        }

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.IsActive = request.IsActive;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated instructor {Id}", id);

        return MapToResponse(instructor);
    }

    public async Task<List<ClassScheduleResponse>> GetScheduleAsync(
        int instructorId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (from.HasValue)
        {
            var fromDate = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(cs => cs.StartTime >= fromDate);
        }

        if (to.HasValue)
        {
            var toDate = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(cs => cs.StartTime <= toDate);
        }

        var schedules = await query.OrderBy(cs => cs.StartTime).ToListAsync(ct);
        return schedules.Select(MapScheduleToResponse).ToList();
    }

    private static InstructorResponse MapToResponse(Instructor i) => new(
        i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
        i.Bio, i.Specializations, i.HireDate, i.IsActive,
        i.CreatedAt, i.UpdatedAt);

    private static ClassScheduleResponse MapScheduleToResponse(ClassSchedule cs) => new(
        cs.Id, cs.ClassTypeId, cs.ClassType.Name,
        cs.InstructorId, $"{cs.Instructor.FirstName} {cs.Instructor.LastName}",
        cs.StartTime, cs.EndTime, cs.Capacity, cs.CurrentEnrollment, cs.WaitlistCount,
        Math.Max(0, cs.Capacity - cs.CurrentEnrollment),
        cs.Room, cs.Status, cs.CancellationReason,
        cs.CreatedAt, cs.UpdatedAt);
}
