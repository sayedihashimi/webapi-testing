using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext context, ILogger<PrescriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto> GetByIdAsync(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id)
            ?? throw new KeyNotFoundException($"Prescription with ID {id} not found.");

        return MapToResponse(prescription);
    }

    public async Task<PrescriptionResponseDto> CreateAsync(CreatePrescriptionDto dto)
    {
        if (!await _context.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId))
            throw new BusinessRuleException($"Medical record with ID {dto.MedicalRecordId} not found.");

        var prescription = new Prescription
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

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", prescription.Id, prescription.MedicalRecordId);

        return MapToResponse(prescription);
    }

    private static PrescriptionResponseDto MapToResponse(Prescription p)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionResponseDto
        {
            Id = p.Id, MedicalRecordId = p.MedicalRecordId,
            MedicationName = p.MedicationName, Dosage = p.Dosage,
            DurationDays = p.DurationDays, StartDate = p.StartDate, EndDate = p.EndDate,
            Instructions = p.Instructions, IsActive = p.EndDate >= today, CreatedAt = p.CreatedAt
        };
    }
}
