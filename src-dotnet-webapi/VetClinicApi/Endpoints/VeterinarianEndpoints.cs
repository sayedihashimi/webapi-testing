using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static void MapVeterinarianEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", async (string? specialization, bool? isAvailable, int page, int pageSize, IVeterinarianService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAllAsync(specialization, isAvailable, page, pageSize, ct));
        })
        .WithName("GetVeterinarians")
        .WithSummary("List veterinarians")
        .WithDescription("Returns a paginated list of veterinarians. Filter by specialization and availability.")
        .Produces<PaginatedResponse<VeterinarianResponse>>();

        group.MapGet("/{id:int}", async (int id, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.GetByIdAsync(id, ct);
            return vet is null ? Results.NotFound() : Results.Ok(vet);
        })
        .WithName("GetVeterinarianById")
        .WithSummary("Get veterinarian details")
        .WithDescription("Returns details for a specific veterinarian.")
        .Produces<VeterinarianResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.CreateAsync(request, ct);
            return Results.Created($"/api/veterinarians/{vet.Id}", vet);
        })
        .WithName("CreateVeterinarian")
        .WithSummary("Add a new veterinarian")
        .WithDescription("Registers a new veterinarian. Email and license number must be unique.")
        .Produces<VeterinarianResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.UpdateAsync(id, request, ct);
            return vet is null ? Results.NotFound() : Results.Ok(vet);
        })
        .WithName("UpdateVeterinarian")
        .WithSummary("Update a veterinarian")
        .WithDescription("Updates all fields for an existing veterinarian.")
        .Produces<VeterinarianResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/schedule", async (int id, DateOnly date, IVeterinarianService service, CancellationToken ct) =>
        {
            var result = await service.GetScheduleAsync(id, date, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetVeterinarianSchedule")
        .WithSummary("Get schedule for a date")
        .WithDescription("Returns all appointments for a veterinarian on a specific date.")
        .Produces<List<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async (int id, AppointmentStatus? status, int page, int pageSize, IVeterinarianService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            var result = await service.GetAppointmentsAsync(id, status, page, pageSize, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetVeterinarianAppointments")
        .WithSummary("Get veterinarian's appointments")
        .WithDescription("Returns paginated appointments for a veterinarian. Optionally filter by status.")
        .Produces<PaginatedResponse<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
