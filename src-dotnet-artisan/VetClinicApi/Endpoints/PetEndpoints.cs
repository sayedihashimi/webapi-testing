using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this RouteGroupBuilder group)
    {
        var pets = group.MapGroup("/pets").WithTags("Pets");

        pets.MapGet("/", GetPets).WithSummary("List all active pets with optional search, species filter, and pagination");
        pets.MapGet("/{id:int}", GetPetById).WithSummary("Get pet by ID including owner info");
        pets.MapPost("/", CreatePet).WithSummary("Create a new pet");
        pets.MapPut("/{id:int}", UpdatePet).WithSummary("Update a pet including owner transfer");
        pets.MapDelete("/{id:int}", SoftDeletePet).WithSummary("Soft-delete a pet (set IsActive = false)");
        pets.MapGet("/{id:int}/medical-records", GetPetMedicalRecords).WithSummary("Get all medical records for a pet");
        pets.MapGet("/{id:int}/vaccinations", GetPetVaccinations).WithSummary("Get all vaccinations for a pet");
        pets.MapGet("/{id:int}/vaccinations/upcoming", GetUpcomingVaccinations).WithSummary("Get vaccinations that are due soon or overdue");
        pets.MapGet("/{id:int}/prescriptions/active", GetActivePrescriptions).WithSummary("Get active prescriptions for a pet");

        return group;
    }

    private static async Task<Ok<PagedResult<PetResponse>>> GetPets(
        IPetService service,
        string? search = null,
        string? species = null,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAllAsync(search, species, includeInactive, page, pageSize, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PetDetailResponse>, NotFound>> GetPetById(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<PetResponse>, BadRequest<ProblemHttpResult>>> CreatePet(
        CreatePetRequest request,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        if (result is null)
        {
            return TypedResults.BadRequest(TypedResults.Problem(
                detail: "Owner not found.",
                statusCode: StatusCodes.Status400BadRequest));
        }

        return TypedResults.Created($"/api/pets/{result.Id}", result);
    }

    private static async Task<Results<Ok<PetResponse>, NotFound>> UpdatePet(
        int id,
        UpdatePetRequest request,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> SoftDeletePet(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var deleted = await service.SoftDeleteAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<MedicalRecordResponse>>> GetPetMedicalRecords(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetMedicalRecordsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<VaccinationResponse>>> GetPetVaccinations(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetVaccinationsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<VaccinationResponse>>> GetUpcomingVaccinations(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetUpcomingVaccinationsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<PrescriptionResponse>>> GetActivePrescriptions(
        int id,
        IPetService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetActivePrescriptionsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }
}
