using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto?> GetByIdAsync(int id)
    {
        var rx = await _db.Prescriptions.FindAsync(id);
        return rx == null ? null : MapToResponse(rx);
    }

    public async Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionDto dto)
    {
        if (!await _db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new KeyNotFoundException($"Medical record with ID {dto.MedicalRecordId} not found.");

        var rx = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddDays(dto.DurationDays),
            Instructions = dto.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(rx);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", rx.Id, rx.MedicalRecordId);
        return MapToResponse(rx);
    }

    public static PrescriptionResponseDto MapToResponse(Prescription p) => new()
    {
        Id = p.Id, MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName, Dosage = p.Dosage,
        DurationDays = p.DurationDays, StartDate = p.StartDate, EndDate = p.EndDate,
        Instructions = p.Instructions, IsActive = p.IsActive, CreatedAt = p.CreatedAt
    };
}
