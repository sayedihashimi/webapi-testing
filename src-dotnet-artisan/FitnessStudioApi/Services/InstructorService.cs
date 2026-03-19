using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class InstructorService(AppDbContext db, ILogger<InstructorService> logger) : IInstructorService
{
    public async Task<IReadOnlyList<InstructorResponse>> GetAllAsync(
        string? specialization, bool? isActive, CancellationToken ct)
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

        return await query
            .OrderBy(i => i.LastName)
            .Select(i => MapToResponse(i))
            .ToListAsync(ct);
    }

    public async Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        return instructor is null ? null : MapToResponse(instructor);
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct)
    {
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
        logger.LogInformation("Created instructor {InstructorName} with Id {InstructorId}",
            $"{instructor.FirstName} {instructor.LastName}", instructor.Id);
        return MapToResponse(instructor);
    }

    public async Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct)
    {
        var instructor = await db.Instructors.FindAsync([id], ct);
        if (instructor is null)
        {
            return null;
        }

        instructor.FirstName = request.FirstName;
        instructor.LastName = request.LastName;
        instructor.Email = request.Email;
        instructor.Phone = request.Phone;
        instructor.Bio = request.Bio;
        instructor.Specializations = request.Specializations;
        instructor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(instructor);
    }

    public async Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(
        int instructorId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct)
    {
        var query = db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .Where(cs => cs.InstructorId == instructorId);

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(cs => cs.StartTime >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(cs => cs.StartTime <= to);
        }

        return await query
            .OrderBy(cs => cs.StartTime)
            .Select(cs => new ClassScheduleResponse(
                cs.Id, cs.ClassTypeId, cs.ClassType.Name,
                cs.InstructorId, cs.Instructor.FirstName + " " + cs.Instructor.LastName,
                cs.StartTime, cs.EndTime, cs.Capacity,
                cs.CurrentEnrollment, cs.WaitlistCount,
                cs.Capacity - cs.CurrentEnrollment,
                cs.Room, cs.Status.ToString(), cs.CancellationReason))
            .ToListAsync(ct);
    }

    private static InstructorResponse MapToResponse(Instructor i) =>
        new(i.Id, i.FirstName, i.LastName, i.Email, i.Phone,
            i.Bio, i.Specializations, i.HireDate, i.IsActive);
}
