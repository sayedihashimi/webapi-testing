using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext context, ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MedicalRecordResponseDto> GetByIdAsync(int id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(dto.AppointmentId)
            ?? throw new BusinessRuleException($"Appointment with ID {dto.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new BusinessRuleException("Medical records can only be created for appointments with status Completed or InProgress.");

        if (await _context.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId))
            throw new BusinessRuleException("A medical record already exists for this appointment.", 409, "Conflict");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId, PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis, Treatment = dto.Treatment, Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate, CreatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        await _context.Entry(record).Reference(m => m.Pet).LoadAsync();
        await _context.Entry(record).Reference(m => m.Veterinarian).LoadAsync();
        return MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _context.SaveChangesAsync();
        return MapToResponse(record);
    }

    public static MedicalRecordResponseDto MapToResponse(MedicalRecord m)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MedicalRecordResponseDto
        {
            Id = m.Id, AppointmentId = m.AppointmentId, PetId = m.PetId, VeterinarianId = m.VeterinarianId,
            Diagnosis = m.Diagnosis, Treatment = m.Treatment, Notes = m.Notes,
            FollowUpDate = m.FollowUpDate, CreatedAt = m.CreatedAt,
            Pet = m.Pet != null ? new PetSummaryDto { Id = m.Pet.Id, Name = m.Pet.Name, Species = m.Pet.Species, Breed = m.Pet.Breed, IsActive = m.Pet.IsActive } : null,
            Veterinarian = m.Veterinarian != null ? new VeterinarianSummaryDto { Id = m.Veterinarian.Id, FirstName = m.Veterinarian.FirstName, LastName = m.Veterinarian.LastName, Specialization = m.Veterinarian.Specialization } : null,
            Prescriptions = m.Prescriptions?.Select(p => new PrescriptionResponseDto
            {
                Id = p.Id, MedicalRecordId = p.MedicalRecordId, MedicationName = p.MedicationName,
                Dosage = p.Dosage, DurationDays = p.DurationDays, StartDate = p.StartDate, EndDate = p.EndDate,
                Instructions = p.Instructions, IsActive = p.EndDate >= today, CreatedAt = p.CreatedAt
            }).ToList() ?? new()
        };
    }
}
