using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this WebApplication app)
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
        .WithDescription("Returns a paginated list of veterinarians with optional filters for specialization and availability.")
        .Produces<PagedResponse<VeterinarianResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async (int id, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.GetByIdAsync(id, ct);
            return vet is null ? Results.NotFound() : Results.Ok(vet);
        })
        .WithName("GetVeterinarianById")
        .WithSummary("Get a veterinarian by ID")
        .WithDescription("Returns detailed information about a specific veterinarian.")
        .Produces<VeterinarianResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
        })
        .WithName("CreateVeterinarian")
        .WithSummary("Create a new veterinarian")
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
        .WithDescription("Updates an existing veterinarian's information.")
        .Produces<VeterinarianResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}/schedule", async (int id, DateOnly date, IVeterinarianService service, CancellationToken ct) =>
        {
            return Results.Ok(await service.GetScheduleAsync(id, date, ct));
        })
        .WithName("GetVeterinarianSchedule")
        .WithSummary("Get a veterinarian's schedule for a date")
        .WithDescription("Returns all appointments for a veterinarian on the specified date.")
        .Produces<List<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async (int id, string? status, int page, int pageSize, IVeterinarianService service, CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;
            return Results.Ok(await service.GetAppointmentsAsync(id, status, page, pageSize, ct));
        })
        .WithName("GetVeterinarianAppointments")
        .WithSummary("Get a veterinarian's appointments")
        .WithDescription("Returns a paginated list of appointments for the specified veterinarian, with optional status filter.")
        .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
