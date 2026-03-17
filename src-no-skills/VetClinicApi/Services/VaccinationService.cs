using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationDto> GetByIdAsync(int id);
    Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto);
}

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<VaccinationDto> GetByIdAsync(int id)
    {
        var vaccination = await _db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new NotFoundException("Vaccination", id);

        return MapToDto(vaccination);
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new NotFoundException("Active Pet", dto.PetId);

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new NotFoundException("Veterinarian", dto.AdministeredByVetId);

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new BusinessRuleException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes
        };

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();

        await _db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync();
        _logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);
        return MapToDto(vaccination);
    }

    private static VaccinationDto MapToDto(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered, ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber, AdministeredByVetId = v.AdministeredByVetId,
        AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryDto
        {
            Id = v.AdministeredByVet.Id, FirstName = v.AdministeredByVet.FirstName,
            LastName = v.AdministeredByVet.LastName, Specialization = v.AdministeredByVet.Specialization
        } : null,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon,
        CreatedAt = v.CreatedAt
    };
}
