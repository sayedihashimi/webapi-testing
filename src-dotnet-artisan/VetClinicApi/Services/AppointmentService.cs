using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    public async Task<PagedResult<AppointmentDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status,
        int? vetId, int? petId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments
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

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
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
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<AppointmentDto>(items, totalCount, page, pageSize);
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        var petSummary = new PetSummaryDto(
            appointment.Pet.Id, appointment.Pet.Name,
            appointment.Pet.Species, appointment.Pet.Breed, appointment.Pet.IsActive);

        var vetDto = new VeterinarianDto(
            appointment.Veterinarian.Id, appointment.Veterinarian.FirstName,
            appointment.Veterinarian.LastName, appointment.Veterinarian.Email,
            appointment.Veterinarian.Phone, appointment.Veterinarian.Specialization,
            appointment.Veterinarian.LicenseNumber, appointment.Veterinarian.IsAvailable,
            appointment.Veterinarian.HireDate);

        MedicalRecordDto? medicalRecordDto = null;
        if (appointment.MedicalRecord is not null)
        {
            var mr = appointment.MedicalRecord;
            medicalRecordDto = new MedicalRecordDto(
                mr.Id, mr.AppointmentId, mr.PetId, mr.VeterinarianId,
                mr.Diagnosis, mr.Treatment, mr.Notes, mr.FollowUpDate, mr.CreatedAt);
        }

        return new AppointmentDetailDto(
            appointment.Id, appointment.PetId, petSummary,
            appointment.VeterinarianId, vetDto,
            appointment.AppointmentDate, appointment.DurationMinutes, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason,
            appointment.CreatedAt, appointment.UpdatedAt, medicalRecordDto);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = new Appointment
        {
            PetId = dto.PetId,
            VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate,
            DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason,
            Notes = dto.Notes
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        // Reload navigation properties in case they changed
        await db.Entry(appointment).Reference(a => a.Pet).LoadAsync(ct);
        await db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync(ct);

        logger.LogInformation("Updated appointment {AppointmentId}", appointment.Id);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null)
        {
            return null;
        }

        appointment.Status = dto.NewStatus;
        if (dto.NewStatus == AppointmentStatus.Cancelled)
        {
            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", appointment.Id, dto.NewStatus);
        return MapToDto(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetTodayAsync(CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<bool> HasConflictAsync(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId = null, CancellationToken ct = default)
    {
        var appointmentEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync(a =>
            appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            appointmentEnd > a.AppointmentDate, ct);
    }

    public async Task<string?> ValidateStatusTransitionAsync(int id, AppointmentStatus newStatus, CancellationToken ct = default)
    {
        var appointment = await db.Appointments.FindAsync([id], ct);
        if (appointment is null)
        {
            return "Appointment not found.";
        }

        var currentStatus = appointment.Status;

        // Terminal states
        if (currentStatus is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return $"Cannot transition from {currentStatus}. It is a terminal status.";
        }

        var validTransitions = currentStatus switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>()
        };

        if (!validTransitions.Contains(newStatus))
        {
            return $"Cannot transition from {currentStatus} to {newStatus}. Valid transitions: {string.Join(", ", validTransitions)}.";
        }

        // Cannot cancel past appointments
        if (newStatus == AppointmentStatus.Cancelled && appointment.AppointmentDate < DateTime.UtcNow)
        {
            return "Cannot cancel a past appointment.";
        }

        return null;
    }

    private static AppointmentDto MapToDto(Appointment a) =>
        new(a.Id, a.PetId, a.Pet.Name,
            a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
            a.AppointmentDate, a.DurationMinutes, a.Status,
            a.Reason, a.Notes, a.CancellationReason,
            a.CreatedAt, a.UpdatedAt);
}
