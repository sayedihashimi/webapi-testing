using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IAppointmentService
{
    Task<PaginatedResponse<AppointmentSummaryDto>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, string? status, int? vetId, int? petId, int page, int pageSize);
    Task<AppointmentDto> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<List<AppointmentSummaryDto>> GetTodayAsync();
}

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AppointmentSummaryDto>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, string? status, int? vetId, int? petId, int page, int pageSize)
    {
        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).AsQueryable();

        if (dateFrom.HasValue) query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var s))
            query = query.Where(a => a.Status == s);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id, AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes, Status = a.Status,
                Reason = a.Reason,
                Pet = new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
                Veterinarian = new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization }
            }).ToListAsync();

        return new PaginatedResponse<AppointmentSummaryDto>
        {
            Data = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    public async Task<AppointmentDto> GetByIdAsync(int id)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Appointment", id);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new NotFoundException("Active Pet", dto.PetId);

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new NotFoundException("Veterinarian", dto.VeterinarianId);

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new BusinessRuleException("Appointment date must be in the future.");

        await CheckScheduleConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appointment = new Appointment
        {
            PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate, DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason, Notes = dto.Notes
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        await _db.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();
        _logger.LogInformation("Created appointment {AppointmentId} for pet {PetId} with vet {VetId}", appointment.Id, appointment.PetId, appointment.VeterinarianId);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Appointment", id);

        if (appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.NoShow)
            throw new BusinessRuleException($"Cannot update an appointment with status '{appointment.Status}'.");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new NotFoundException("Active Pet", dto.PetId);

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new NotFoundException("Veterinarian", dto.VeterinarianId);

        await CheckScheduleConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _db.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();
        _logger.LogInformation("Updated appointment {AppointmentId}", appointment.Id);
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Appointment", id);

        ValidateStatusTransition(appointment.Status, dto.Status);

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new BusinessRuleException("Cancellation reason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new BusinessRuleException("Cannot cancel a past appointment.");

            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = dto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated appointment {AppointmentId} status to {Status}", appointment.Id, dto.Status);
        return MapToDto(appointment);
    }

    public async Task<List<AppointmentSummaryDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id, AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes, Status = a.Status,
                Reason = a.Reason,
                Pet = new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
                Veterinarian = new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization }
            }).ToListAsync();
    }

    private async Task CheckScheduleConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeId)
    {
        var endTime = appointmentDate.AddMinutes(durationMinutes);

        var conflicting = await _db.Appointments
            .Where(a => a.VeterinarianId == vetId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow &&
                        (excludeId == null || a.Id != excludeId))
            .ToListAsync();

        var hasConflict = conflicting.Any(a =>
        {
            var existingEnd = a.AppointmentDate.AddMinutes(a.DurationMinutes);
            return appointmentDate < existingEnd && endTime > a.AppointmentDate;
        });

        if (hasConflict)
            throw new BusinessRuleException("The veterinarian has a scheduling conflict for the requested time.", StatusCodes.Status409Conflict);
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus target)
    {
        var allowed = current switch
        {
            AppointmentStatus.Scheduled => new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow },
            AppointmentStatus.CheckedIn => new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled },
            AppointmentStatus.InProgress => new[] { AppointmentStatus.Completed },
            _ => Array.Empty<AppointmentStatus>()
        };

        if (!allowed.Contains(target))
            throw new BusinessRuleException($"Cannot transition from '{current}' to '{target}'. Allowed transitions: {string.Join(", ", allowed)}.");
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id, PetId = a.PetId, VeterinarianId = a.VeterinarianId,
        AppointmentDate = a.AppointmentDate, DurationMinutes = a.DurationMinutes,
        Status = a.Status, Reason = a.Reason, Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt,
        Pet = a.Pet != null ? new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive } : null,
        Veterinarian = a.Veterinarian != null ? new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization } : null,
        MedicalRecord = a.MedicalRecord != null ? new MedicalRecordSummaryDto { Id = a.MedicalRecord.Id, Diagnosis = a.MedicalRecord.Diagnosis, Treatment = a.MedicalRecord.Treatment, FollowUpDate = a.MedicalRecord.FollowUpDate } : null
    };
}
