using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static void MapVeterinarianEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", async Task<Ok<PaginatedResponse<VeterinarianResponse>>> (
            IVeterinarianService service,
            string? specialization,
            bool? isAvailable,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var result = await service.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetVeterinarians")
        .WithSummary("List all veterinarians")
        .WithDescription("Returns a paginated list of veterinarians. Supports filter by specialization and availability.")
        .Produces<PaginatedResponse<VeterinarianResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<VeterinarianResponse>, NotFound>> (
            int id, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.GetByIdAsync(id, ct);
            return vet is null ? TypedResults.NotFound() : TypedResults.Ok(vet);
        })
        .WithName("GetVeterinarianById")
        .WithSummary("Get a veterinarian by ID")
        .WithDescription("Returns full details for the specified veterinarian.")
        .Produces<VeterinarianResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<VeterinarianResponse>> (
            CreateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
        })
        .WithName("CreateVeterinarian")
        .WithSummary("Create a new veterinarian")
        .WithDescription("Creates a new veterinarian. Email and license number must be unique.")
        .Produces<VeterinarianResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<VeterinarianResponse>, NotFound>> (
            int id, UpdateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.UpdateAsync(id, request, ct);
            return vet is null ? TypedResults.NotFound() : TypedResults.Ok(vet);
        })
        .WithName("UpdateVeterinarian")
        .WithSummary("Update a veterinarian")
        .WithDescription("Updates all fields of an existing veterinarian.")
        .Produces<VeterinarianResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/schedule", async Task<Results<Ok<IReadOnlyList<AppointmentResponse>>, NotFound>> (
            int id, IVeterinarianService service, DateOnly date, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, date, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetVeterinarianSchedule")
        .WithSummary("Get a veterinarian's schedule for a date")
        .WithDescription("Returns all non-cancelled appointments for the veterinarian on the specified date.")
        .Produces<IReadOnlyList<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, NotFound>> (
            int id, IVeterinarianService service, string? status, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);
            var appointments = await service.GetAppointmentsAsync(id, status, page, pageSize, ct);
            return TypedResults.Ok(appointments);
        })
        .WithName("GetVeterinarianAppointments")
        .WithSummary("Get appointments for a veterinarian")
        .WithDescription("Returns paginated appointments for the veterinarian. Supports filter by status.")
        .Produces<PaginatedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
