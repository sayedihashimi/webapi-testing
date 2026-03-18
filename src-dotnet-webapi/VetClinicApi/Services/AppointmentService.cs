using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
{
    private static readonly HashSet<AppointmentStatus> ActiveStatuses = [
        AppointmentStatus.Scheduled,
        AppointmentStatus.CheckedIn,
        AppointmentStatus.InProgress
    ];

    public async Task<PagedResponse<AppointmentResponse>> GetAllAsync(
        DateOnly? date, string? status, int? vetId, int? petId,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .AsQueryable();

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue);
            var end = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);

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
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<AppointmentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return appointment is null ? null : MapToResponse(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
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
            Status = AppointmentStatus.Scheduled,
            Reason = request.Reason,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        var created = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstAsync(a => a.Id == appointment.Id, ct);

        logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}",
            appointment.Id, appointment.PetId, appointment.VeterinarianId);

        return MapToResponse(created);
    }

    public async Task<AppointmentResponse?> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update appointment with status '{appointment.Status}'.");

        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Id == request.VeterinarianId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.VeterinarianId} not found.");

        await CheckSchedulingConflict(request.VeterinarianId, request.AppointmentDate, request.DurationMinutes, id, ct);

        appointment.PetId = request.PetId;
        appointment.VeterinarianId = request.VeterinarianId;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Reason = request.Reason;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var updated = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstAsync(a => a.Id == id, ct);

        logger.LogInformation("Updated appointment {AppointmentId}", id);

        return MapToResponse(updated);
    }

    public async Task<AppointmentResponse?> UpdateStatusAsync(int id, UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return null;

        ValidateStatusTransition(appointment.Status, request.Status);

        if (request.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                throw new ArgumentException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new ArgumentException("Cannot cancel a past appointment.");

            appointment.CancellationReason = request.CancellationReason;
        }

        appointment.Status = request.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", id, request.Status);

        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponse>> GetTodayAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.ToDateTime(TimeOnly.MinValue);
        var end = today.ToDateTime(TimeOnly.MaxValue);

        var appointments = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(ct);

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId, CancellationToken ct)
    {
        var appointmentEnd = appointmentDate.AddMinutes(durationMinutes);

        var query = db.Appointments.AsNoTracking()
            .Where(a => a.VeterinarianId == vetId
                && ActiveStatuses.Contains(a.Status)
                && a.AppointmentDate < appointmentEnd
                && appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes));

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        var conflict = await query.AnyAsync(ct);
        if (conflict)
            throw new InvalidOperationException("The veterinarian has a scheduling conflict at the requested time.");
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
            [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.Cancelled],
            [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(next))
            throw new ArgumentException($"Cannot transition from '{current}' to '{next}'.");
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        VeterinarianId = a.VeterinarianId,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Pet = a.Pet != null ? new PetSummaryResponse
        {
            Id = a.Pet.Id,
            Name = a.Pet.Name,
            Species = a.Pet.Species,
            Breed = a.Pet.Breed,
            IsActive = a.Pet.IsActive
        } : null,
        Veterinarian = a.Veterinarian != null ? new VeterinarianSummaryResponse
        {
            Id = a.Veterinarian.Id,
            FirstName = a.Veterinarian.FirstName,
            LastName = a.Veterinarian.LastName,
            Specialization = a.Veterinarian.Specialization
        } : null,
        MedicalRecord = a.MedicalRecord != null ? new MedicalRecordSummaryResponse
        {
            Id = a.MedicalRecord.Id,
            Diagnosis = a.MedicalRecord.Diagnosis,
            Treatment = a.MedicalRecord.Treatment,
            CreatedAt = a.MedicalRecord.CreatedAt
        } : null
    };
}
