using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext context, ILogger<VaccinationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VaccinationResponseDto> GetByIdAsync(int id)
    {
        var vaccination = await _context.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vaccination with ID {id} not found.");

        return MapToResponse(vaccination);
    }

    public async Task<VaccinationResponseDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");

        if (!await _context.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new BusinessRuleException($"Veterinarian with ID {dto.AdministeredByVetId} not found.");

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new BusinessRuleException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = dto.PetId, VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered, ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber, AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes, CreatedAt = DateTime.UtcNow
        };

        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Vaccination recorded: {VaccinationId} for Pet {PetId}", vaccination.Id, vaccination.PetId);

        await _context.Entry(vaccination).Reference(v => v.Pet).LoadAsync();
        await _context.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync();
        return MapToResponse(vaccination);
    }

    public static VaccinationResponseDto MapToResponse(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered, ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber, AdministeredByVetId = v.AdministeredByVetId,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon, CreatedAt = v.CreatedAt,
        Pet = v.Pet != null ? new PetSummaryDto { Id = v.Pet.Id, Name = v.Pet.Name, Species = v.Pet.Species, Breed = v.Pet.Breed, IsActive = v.Pet.IsActive } : null,
        AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryDto { Id = v.AdministeredByVet.Id, FirstName = v.AdministeredByVet.FirstName, LastName = v.AdministeredByVet.LastName, Specialization = v.AdministeredByVet.Specialization } : null
    };
}
