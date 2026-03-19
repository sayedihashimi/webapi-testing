using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> s_activeStatuses =
        [AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn, AppointmentStatus.InProgress];

    public async Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, string? status, int? vetId, int? petId,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian).AsQueryable();

        if (dateFrom.HasValue) query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => MapToResponse(a))
            .ToListAsync(ct);

        return PaginatedResponse<AppointmentResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var a = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Prescriptions)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Pet)
            .Include(a => a.MedicalRecord).ThenInclude(m => m!.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (a is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        MedicalRecordResponse? medRecord = null;
        if (a.MedicalRecord is not null)
        {
            var m = a.MedicalRecord;
            medRecord = new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId, m.Pet.Name, m.VeterinarianId,
                $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
                m.Prescriptions.Select(p => new PrescriptionResponse(
                    p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage, p.DurationDays,
                    p.StartDate, p.EndDate, p.Instructions, p.EndDate >= today, p.CreatedAt)).ToList());
        }

        return new AppointmentDetailResponse(
            a.Id, a.PetId, a.Pet.Name, a.Pet.Species, a.Pet.OwnerId,
            $"{a.Pet.Owner.FirstName} {a.Pet.Owner.LastName}",
            a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes,
            a.CancellationReason, medRecord, a.CreatedAt, a.UpdatedAt);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");
        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        var created = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appointment.Id, ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId}", appointment.Id, appointment.PetId);
        return MapToResponse(created);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update appointment in {appointment.Status} status.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (appointment.AppointmentDate != request.AppointmentDate || appointment.VeterinarianId != request.VeterinarianId
            || appointment.DurationMinutes != request.DurationMinutes)
        {
            await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);
        }

        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.VeterinarianId = request.VeterinarianId;
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

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var newStart = appointmentDate;
        var newEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId && s_activeStatuses.Contains(a.Status));

        if (excludeId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeId.Value);

        var hasConflict = await conflictQuery.AnyAsync(a =>
            newStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) && newEnd > a.AppointmentDate, ct);

        if (hasConflict)
            throw new InvalidOperationException("Veterinarian has a scheduling conflict at the requested time.");
    }

    private static void ValidateStatusTransition(Appointment appointment, UpdateAppointmentStatusRequest request)
    {
        var allowed = appointment.Status switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>()
        };

        if (!allowed.Contains(request.Status))
            throw new ArgumentException($"Cannot transition from {appointment.Status} to {request.Status}.");

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("Cancellation reason is required.");
            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new ArgumentException("Past appointments cannot be cancelled.");
        }
    }

    private static AppointmentResponse MapToResponse(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
            $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes,
            a.CancellationReason, a.CreatedAt, a.UpdatedAt);
}
