using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Prescription;
using VetClinicApi.Models;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PrescriptionDto> GetByIdAsync(int id)
    {
        var prescription = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Prescription with ID {id} not found");

        return MapToDto(prescription);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto)
    {
        if (!await _db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new KeyNotFoundException($"Medical record with ID {dto.MedicalRecordId} not found");

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            Instructions = dto.Instructions
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}", prescription.Id, dto.MedicalRecordId);
        return MapToDto(prescription);
    }

    private static PrescriptionDto MapToDto(Prescription p) => new()
    {
        Id = p.Id, MedicalRecordId = p.MedicalRecordId,
        MedicationName = p.MedicationName, Dosage = p.Dosage,
        DurationDays = p.DurationDays, StartDate = p.StartDate,
        EndDate = p.EndDate, Instructions = p.Instructions,
        IsActive = p.IsActive, CreatedAt = p.CreatedAt
    };
}
