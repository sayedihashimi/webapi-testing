using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> TerminalStatuses =
        [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow];

    public async Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian).AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
                .ThenInclude(m => m!.Prescriptions)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        MedicalRecordResponse? medicalRecord = null;
        if (appointment.MedicalRecord is not null)
        {
            var m = appointment.MedicalRecord;
            medicalRecord = new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId, appointment.Pet.Name,
                m.VeterinarianId,
                appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt);
        }

        return new AppointmentDetailResponse(
            appointment.Id, appointment.PetId, appointment.Pet.Name,
            appointment.VeterinarianId,
            appointment.Veterinarian.FirstName + " " + appointment.Veterinarian.LastName,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, medicalRecord);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckConflictAsync(request.VeterinarianId, request.AppointmentDate,
            request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        var created = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appointment.Id, ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return MapToResponse(created);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"Cannot update appointment in '{appointment.Status}' status.");

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate != appointment.AppointmentDate ||
            request.VeterinarianId != appointment.VeterinarianId ||
            request.DurationMinutes != appointment.DurationMinutes)
        {
            await CheckConflictAsync(request.VeterinarianId, request.AppointmentDate,
                request.DurationMinutes, id, ct);
        }

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var updated = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == id, ct);

        logger.LogInformation("Updated appointment {AppointmentId}", id);
        return MapToResponse(updated);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        ValidateStatusTransition(appointment, request);

        appointment.Status = request.Status;
        if (request.Status == AppointmentStatus.Cancelled)
            appointment.CancellationReason = request.CancellationReason;

        appointment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", id, request.Status);
        return MapToResponse(appointment);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);
    }

    private async Task CheckConflictAsync(int vetId, DateTime appointmentDate,
        int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var newStart = appointmentDate;
        var newEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeId.Value);

        // Load potential conflicts and check overlap in memory for SQLite compatibility
        var existingAppointments = await conflictQuery
            .Select(a => new { a.AppointmentDate, a.DurationMinutes })
            .ToListAsync(ct);

        var hasConflict = existingAppointments.Any(a =>
        {
            var existingEnd = a.AppointmentDate.AddMinutes(a.DurationMinutes);
            return newStart < existingEnd && newEnd > a.AppointmentDate;
        });

        if (hasConflict)
            throw new InvalidOperationException(
                "The veterinarian has a conflicting appointment at this time.");
    }

    private static void ValidateStatusTransition(Appointment appointment, UpdateAppointmentStatusRequest request)
    {
        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"Cannot change status from terminal state '{appointment.Status}'.");

        var validTransitions = appointment.Status switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>()
        };

        if (!validTransitions.Contains(request.Status))
            throw new ArgumentException(
                $"Invalid status transition from '{appointment.Status}' to '{request.Status}'. " +
                $"Valid transitions: {string.Join(", ", validTransitions)}");

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("Cancellation reason is required.");

            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new ArgumentException("Cannot cancel past appointments.");
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
