using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
{
    public async Task<PagedResponse<AppointmentResponse>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (vetId.HasValue)
        {
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        }

        if (petId.HasValue)
        {
            query = query.Where(a => a.PetId == petId.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>(
            items, page, pageSize, totalCount, totalPages,
            page < totalPages, page > 1);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appt = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appt is null)
        {
            return null;
        }

        MedicalRecordResponse? medRecord = null;
        if (appt.MedicalRecord is not null)
        {
            var m = appt.MedicalRecord;
            medRecord = PetService.MapToMedicalRecordResponse(m);
        }

        return new AppointmentDetailResponse(
            appt.Id, appt.PetId, appt.Pet.Name, appt.Pet.Species,
            appt.Pet.OwnerId, appt.Pet.Owner.FirstName + " " + appt.Pet.Owner.LastName,
            appt.VeterinarianId, appt.Veterinarian.FirstName + " " + appt.Veterinarian.LastName,
            appt.AppointmentDate, appt.DurationMinutes, appt.Status,
            appt.Reason, appt.Notes, appt.CancellationReason,
            appt.CreatedAt, appt.UpdatedAt, medRecord);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (request.AppointmentDate <= DateTime.UtcNow)
        {
            throw new ArgumentException("Appointment date must be in the future.");
        }

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");
        }

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
        {
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");
        }

        await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

        var appt = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes
        };

        db.Appointments.Add(appt);
        await db.SaveChangesAsync(ct);

        var created = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appt.Id, ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}",
            appt.Id, appt.PetId, appt.VeterinarianId);

        return MapToResponse(created);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appt = await db.Appointments.FindAsync([id], ct);
        if (appt is null)
        {
            return null;
        }

        if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            throw new InvalidOperationException($"Cannot update appointment with status '{appt.Status}'.");
        }

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");
        }

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
        {
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");
        }

        // Re-check conflicts if date/time/vet changed
        if (appt.AppointmentDate != request.AppointmentDate ||
            appt.VeterinarianId != request.VeterinarianId ||
            appt.DurationMinutes != request.DurationMinutes)
        {
            await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);
        }

        appt.PetId = request.PetId;
        appt.VeterinarianId = request.VeterinarianId;
        appt.AppointmentDate = request.AppointmentDate;
        appt.DurationMinutes = request.DurationMinutes;
        appt.Reason = request.Reason;
        appt.Notes = request.Notes;
        appt.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var updated = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appt.Id, ct);

        logger.LogInformation("Updated appointment {AppointmentId}", id);
        return MapToResponse(updated);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appt = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appt is null)
        {
            return null;
        }

        ValidateStatusTransition(appt, request.Status);

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
            {
                throw new ArgumentException("CancellationReason is required when cancelling an appointment.");
            }

            if (appt.AppointmentDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel a past appointment.");
            }

            appt.CancellationReason = request.CancellationReason;
        }

        appt.Status = request.Status;
        appt.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", id, request.Status);
        return MapToResponse(appt);
    }

    public async Task<List<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    private async Task CheckSchedulingConflictAsync(
        int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var endTime = appointmentDate.AddMinutes(durationMinutes);

        var query = db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        var hasConflict = await query.AnyAsync(a =>
            appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            endTime > a.AppointmentDate, ct);

        if (hasConflict)
        {
            throw new InvalidOperationException("The veterinarian has a scheduling conflict at the requested time.");
        }
    }

    private static void ValidateStatusTransition(Appointment appt, AppointmentStatus newStatus)
    {
        var allowed = appt.Status switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>()
        };

        if (!allowed.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from '{appt.Status}' to '{newStatus}'. " +
                $"Allowed transitions: {(allowed.Length > 0 ? string.Join(", ", allowed) : "none (terminal state)")}.");
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
