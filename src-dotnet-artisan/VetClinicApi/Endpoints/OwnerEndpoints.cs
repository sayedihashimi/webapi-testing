using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", GetAll)
            .WithName("GetOwners")
            .WithSummary("Get all owners with optional search and pagination");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetOwnerById")
            .WithSummary("Get owner by ID with pets");

        group.MapPost("/", Create)
            .WithName("CreateOwner")
            .WithSummary("Create a new owner");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateOwner")
            .WithSummary("Update an existing owner");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteOwner")
            .WithSummary("Delete an owner (fails if active pets exist)");

        group.MapGet("/{id:int}/pets", GetPets)
            .WithName("GetOwnerPets")
            .WithSummary("Get all pets belonging to an owner");

        group.MapGet("/{id:int}/appointments", GetAppointments)
            .WithName("GetOwnerAppointments")
            .WithSummary("Get all appointments for an owner's pets");

        return group;
    }

    private static async Task<Ok<PagedResponse<OwnerResponse>>> GetAll(
        OwnerService service, string? search = null, int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAllAsync(search, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OwnerDetailResponse>, NotFound>> GetById(
        int id, OwnerService service, CancellationToken ct)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }

    private static async Task<Created<OwnerResponse>> Create(
        CreateOwnerRequest request, OwnerService service, CancellationToken ct)
    {
        var owner = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/owners/{owner.Id}", owner);
    }

    private static async Task<Results<Ok<OwnerResponse>, NotFound>> Update(
        int id, UpdateOwnerRequest request, OwnerService service, CancellationToken ct)
    {
        var owner = await service.UpdateAsync(id, request, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        int id, OwnerService service, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<Ok<List<PetResponse>>> GetPets(
        int id, OwnerService service, CancellationToken ct)
    {
        var pets = await service.GetPetsAsync(id, ct);
        return TypedResults.Ok(pets);
    }

    private static async Task<Ok<PagedResponse<AppointmentResponse>>> GetAppointments(
        int id, OwnerService service, int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAppointmentsAsync(id, page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}
