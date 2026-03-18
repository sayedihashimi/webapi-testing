using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", async (string? search, string? species, bool? includeInactive, int page, int pageSize, IPetService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(search, species, includeInactive ?? false, page, pageSize, ct));
        })
        .WithName("GetPets")
        .WithSummary("List pets")
        .WithDescription("Returns a paginated list of active pets. Use includeInactive=true to include deactivated pets.")
        .Produces<PaginatedResponse<PetResponse>>();

        group.MapGet("/{id:int}", async (int id, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.GetByIdAsync(id, ct);
            return pet is null ? Results.NotFound() : Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get pet details")
        .WithDescription("Returns pet details including owner information.")
        .Produces<PetDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.CreateAsync(request, ct);
            return Results.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Register a new pet")
        .WithDescription("Registers a new pet. Must specify a valid owner ID.")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async (int id, UpdatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.UpdateAsync(id, request, ct);
            return pet is null ? Results.NotFound() : Results.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .WithDescription("Updates pet details. Changing OwnerId transfers ownership.")
        .Produces<PetResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, IPetService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeletePet")
        .WithSummary("Deactivate a pet")
        .WithDescription("Soft-deletes a pet by setting IsActive to false.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/medical-records", async (int id, int page, int pageSize, IPetService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            var result = await service.GetMedicalRecordsAsync(id, page, pageSize, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPetMedicalRecords")
        .WithSummary("Get pet's medical records")
        .WithDescription("Returns paginated medical records for the specified pet.")
        .Produces<PaginatedResponse<MedicalRecordResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations", async (int id, IPetService service, CancellationToken ct) =>
        {
            var result = await service.GetVaccinationsAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPetVaccinations")
        .WithSummary("Get pet's vaccinations")
        .WithDescription("Returns all vaccination records for the specified pet.")
        .Produces<List<VaccinationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations/upcoming", async (int id, IPetService service, CancellationToken ct) =>
        {
            var result = await service.GetUpcomingVaccinationsAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPetUpcomingVaccinations")
        .WithSummary("Get due/overdue vaccinations")
        .WithDescription("Returns vaccinations that are expired or expiring within 30 days.")
        .Produces<List<VaccinationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/prescriptions/active", async (int id, IPetService service, CancellationToken ct) =>
        {
            var result = await service.GetActivePrescriptionsAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPetActivePrescriptions")
        .WithSummary("Get active prescriptions")
        .WithDescription("Returns currently active prescriptions for the specified pet.")
        .Produces<List<PrescriptionResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
