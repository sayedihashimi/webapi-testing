using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext context, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> s_validTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = [],
    };

    public async Task<PagedResult<AppointmentResponse>> GetAllAsync(DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
        {
            query = query.Where(a => a.Status == statusEnum);
        }

        if (vetId.HasValue)
        {
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        }

        if (petId.HasValue)
        {
            query = query.Where(a => a.PetId == petId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentResponse>(items, totalCount, page, pageSize);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var appointment = await context.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Pet)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (appointment is null)
        {
            return null;
        }

        MedicalRecordResponse? medicalRecord = null;
        if (appointment.MedicalRecord is not null)
        {
            var mr = appointment.MedicalRecord;
            medicalRecord = new MedicalRecordResponse(
                mr.Id, mr.AppointmentId, mr.PetId, mr.Pet.Name,
                mr.VeterinarianId, $"{mr.Veterinarian.FirstName} {mr.Veterinarian.LastName}",
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate,
                mr.CreatedAt);
        }

        return new AppointmentDetailResponse(
            appointment.Id, appointment.PetId, appointment.Pet.Name, appointment.Pet.Species,
            appointment.Pet.OwnerId, $"{appointment.Pet.Owner.FirstName} {appointment.Pet.Owner.LastName}",
            appointment.VeterinarianId, $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}",
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt,
            medicalRecord);
    }

    public async Task<(AppointmentResponse? Result, string? Error)> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            return (null, "Appointment date must be in the future.");
        }

        var petExists = await context.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, cancellationToken);
        if (!petExists)
        {
            return (null, "Pet not found or is inactive.");
        }

        var vetExists = await context.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, cancellationToken);
        if (!vetExists)
        {
            return (null, "Veterinarian not found.");
        }

        var conflictError = await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, cancellationToken);
        if (conflictError is not null)
        {
            return (null, conflictError);
        }

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(appointment).Reference(a => a.Pet).LoadAsync(cancellationToken);
        await context.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(cancellationToken);

        logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return (MapToResponse(appointment), null);
    }

    public async Task<(AppointmentResponse? Result, string? Error, bool NotFound)> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (appointment is null)
        {
            return (null, null, true);
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return (null, $"Cannot update an appointment with status '{appointment.Status}'.", false);
        }

        var petExists = await context.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, cancellationToken);
        if (!petExists)
        {
            return (null, "Pet not found or is inactive.", false);
        }

        var vetExists = await context.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, cancellationToken);
        if (!vetExists)
        {
            return (null, "Veterinarian not found.", false);
        }

        if (request.AppointmentDate != appointment.AppointmentDate ||
            request.VeterinarianId != appointment.VeterinarianId ||
            request.DurationMinutes != appointment.DurationMinutes)
        {
            var conflictError = await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, cancellationToken);
            if (conflictError is not null)
            {
                return (null, conflictError, false);
            }
        }

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;

        await context.SaveChangesAsync(cancellationToken);
        await context.Entry(appointment).Reference(a => a.Pet).LoadAsync(cancellationToken);
        await context.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(cancellationToken);

        logger.LogInformation("Appointment updated: {AppointmentId}", appointment.Id);

        return (MapToResponse(appointment), null, false);
    }

    public async Task<(bool Success, string? Error, bool NotFound)> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        var appointment = await context.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
        {
            return (false, null, true);
        }

        if (!s_validTransitions.TryGetValue(appointment.Status, out var validTargets) ||
            !validTargets.Contains(request.Status))
        {
            return (false, $"Cannot transition from '{appointment.Status}' to '{request.Status}'.", false);
        }

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
            {
                return (false, "CancellationReason is required when cancelling an appointment.", false);
            }

            if (appointment.AppointmentDate < DateTime.UtcNow)
            {
                return (false, "Cannot cancel a past appointment.", false);
            }

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", appointment.Id, appointment.Status);

        return (true, null, false);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<string?> CheckSchedulingConflictAsync(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken cancellationToken)
    {
        var proposedStart = appointmentDate;
        var proposedEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = context.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        var hasConflict = await query.AnyAsync(a =>
            proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            proposedEnd > a.AppointmentDate, cancellationToken);

        return hasConflict ? "Scheduling conflict: the veterinarian has an overlapping appointment." : null;
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name,
            a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
