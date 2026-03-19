using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<OwnerResponse>>, BadRequest>> (
            string? search, int? page, int? pageSize,
            IOwnerService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOwners")
        .WithSummary("List all owners")
        .WithDescription("Returns a paginated list of owners. Optionally filter by name or email.");

        group.MapGet("/{id:int}", async Task<Results<Ok<OwnerDetailResponse>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get owner by ID")
        .WithDescription("Returns owner details including their pets.");

        group.MapPost("/", async Task<Results<Created<OwnerResponse>, Conflict<ProblemDetails>>> (
            CreateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/owners/{owner.Id}", owner);
        })
        .WithName("CreateOwner")
        .WithSummary("Create a new owner")
        .WithDescription("Creates a new pet owner. Email must be unique.");

        group.MapPut("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound>> (
            int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.UpdateAsync(id, request, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("UpdateOwner")
        .WithSummary("Update an owner")
        .WithDescription("Updates all fields of an existing owner.");

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteOwner")
        .WithSummary("Delete an owner")
        .WithDescription("Deletes an owner. Fails if the owner has active pets.");

        group.MapGet("/{id:int}/pets", async Task<Results<Ok<IReadOnlyList<PetSummaryResponse>>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var pets = await service.GetPetsAsync(id, ct);
            return TypedResults.Ok(pets);
        })
        .WithName("GetOwnerPets")
        .WithSummary("Get owner's pets")
        .WithDescription("Returns all pets belonging to the specified owner.");

        group.MapGet("/{id:int}/appointments", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, NotFound>> (
            int id, int? page, int? pageSize,
            IOwnerService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAppointmentsAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOwnerAppointments")
        .WithSummary("Get owner's appointment history")
        .WithDescription("Returns paginated appointments for all of the owner's pets.");

        return group;
    }
}
