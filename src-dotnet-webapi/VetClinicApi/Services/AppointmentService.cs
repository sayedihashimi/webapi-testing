using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(DateTime? date, AppointmentStatus? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct);
    Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct);
    Task<List<AppointmentResponse>> GetTodayAsync(CancellationToken ct);
}

public class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    public async Task<PaginatedResponse<AppointmentResponse>> GetAllAsync(DateTime? date, AppointmentStatus? status, int? vetId, int? petId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .AsQueryable();

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(a => a.AppointmentDate >= start && a.AppointmentDate < end);
        }

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (vetId.HasValue)
            query = query.Where(a => a.VeterinarianId == vetId.Value);

        if (petId.HasValue)
            query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<AppointmentResponse>(items, page, pageSize, totalCount, totalPages, page < totalPages, page > 1);
    }

    public async Task<AppointmentDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .Where(a => a.Id == id)
            .Select(a => new AppointmentDetailResponse(
                a.Id, a.PetId, a.Pet.Name, a.Pet.Species, a.Pet.OwnerId,
                $"{a.Pet.Owner.FirstName} {a.Pet.Owner.LastName}",
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt,
                a.MedicalRecord == null ? null : new MedicalRecordResponse(
                    a.MedicalRecord.Id, a.MedicalRecord.AppointmentId, a.MedicalRecord.PetId, a.Pet.Name,
                    a.MedicalRecord.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                    a.MedicalRecord.Diagnosis, a.MedicalRecord.Treatment, a.MedicalRecord.Notes,
                    a.MedicalRecord.FollowUpDate, a.MedicalRecord.CreatedAt)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, null, ct);

        var appointment = new Appointment
        {
            PetId = request.PetId,
            VeterinarianId = request.VeterinarianId,
            AppointmentDate = request.AppointmentDate,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        var pet = await db.Pets.FindAsync([appointment.PetId], ct);
        var vet = await db.Veterinarians.FindAsync([appointment.VeterinarianId], ct);
        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId}", appointment.Id, appointment.PetId);

        return new AppointmentResponse(appointment.Id, appointment.PetId, pet!.Name, appointment.VeterinarianId,
            $"{vet!.FirstName} {vet.LastName}",
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null) return null;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update appointment in {appointment.Status} status.");

        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.VeterinarianId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var pet = await db.Pets.FindAsync([appointment.PetId], ct);
        var vet = await db.Veterinarians.FindAsync([appointment.VeterinarianId], ct);
        logger.LogInformation("Updated appointment {AppointmentId}", id);

        return new AppointmentResponse(appointment.Id, appointment.PetId, pet!.Name, appointment.VeterinarianId,
            $"{vet!.FirstName} {vet.LastName}",
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appointment is null) return null;

        ValidateStatusTransition(appointment.Status, request.Status);

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot cancel a past appointment.");

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", id, request.Status);

        return new AppointmentResponse(appointment.Id, appointment.PetId, appointment.Pet.Name,
            appointment.VeterinarianId, $"{appointment.Veterinarian.FirstName} {appointment.Veterinarian.LastName}",
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt);
    }

    public async Task<List<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    private async Task CheckSchedulingConflictAsync(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var endTime = appointmentDate.AddMinutes(durationMinutes);

        var conflictQuery = db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
            conflictQuery = conflictQuery.Where(a => a.Id != excludeAppointmentId.Value);

        var existingAppointments = await conflictQuery
            .Select(a => new { a.AppointmentDate, a.DurationMinutes })
            .ToListAsync(ct);

        foreach (var existing in existingAppointments)
        {
            var existingEnd = existing.AppointmentDate.AddMinutes(existing.DurationMinutes);
            if (appointmentDate < existingEnd && endTime > existing.AppointmentDate)
                throw new InvalidOperationException("Scheduling conflict: the veterinarian already has an appointment during this time.");
        }
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        var valid = current switch
        {
            AppointmentStatus.Scheduled => next is AppointmentStatus.CheckedIn or AppointmentStatus.Cancelled or AppointmentStatus.NoShow,
            AppointmentStatus.CheckedIn => next is AppointmentStatus.InProgress or AppointmentStatus.Cancelled,
            AppointmentStatus.InProgress => next is AppointmentStatus.Completed,
            _ => false
        };

        if (!valid)
            throw new ArgumentException($"Cannot transition from {current} to {next}.");
    }
}
