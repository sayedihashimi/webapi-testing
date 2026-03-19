using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this RouteGroupBuilder group)
    {
        var owners = group.MapGroup("/owners").WithTags("Owners");

        owners.MapGet("/", GetOwners).WithSummary("List all owners with optional search and pagination");
        owners.MapGet("/{id:int}", GetOwnerById).WithSummary("Get owner by ID including their pets");
        owners.MapPost("/", CreateOwner).WithSummary("Create a new owner");
        owners.MapPut("/{id:int}", UpdateOwner).WithSummary("Update an existing owner");
        owners.MapDelete("/{id:int}", DeleteOwner).WithSummary("Delete an owner (fails if owner has active pets)");
        owners.MapGet("/{id:int}/pets", GetOwnerPets).WithSummary("Get all pets for an owner");
        owners.MapGet("/{id:int}/appointments", GetOwnerAppointments).WithSummary("Get appointment history for all of an owner's pets");

        return group;
    }

    private static async Task<Ok<PagedResult<OwnerResponse>>> GetOwners(
        IOwnerService service,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAllAsync(search, page, pageSize, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OwnerDetailResponse>, NotFound>> GetOwnerById(
        int id,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Created<OwnerResponse>> CreateOwner(
        CreateOwnerRequest request,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/owners/{result.Id}", result);
    }

    private static async Task<Results<Ok<OwnerResponse>, NotFound>> UpdateOwner(
        int id,
        UpdateOwnerRequest request,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<ProblemHttpResult>>> DeleteOwner(
        int id,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var (found, hasActivePets) = await service.DeleteAsync(id, cancellationToken);

        if (!found)
        {
            return TypedResults.NotFound();
        }

        if (hasActivePets)
        {
            return TypedResults.Conflict(TypedResults.Problem(
                detail: "Cannot delete owner with active pets.",
                statusCode: StatusCodes.Status409Conflict));
        }

        return TypedResults.NoContent();
    }

    private static async Task<Ok<IReadOnlyList<PetResponse>>> GetOwnerPets(
        int id,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPetsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<AppointmentResponse>>> GetOwnerAppointments(
        int id,
        IOwnerService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAppointmentsAsync(id, cancellationToken);
        return TypedResults.Ok(result);
    }
}
