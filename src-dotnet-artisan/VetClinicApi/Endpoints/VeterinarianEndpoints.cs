using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/veterinarians")
            .WithTags("Veterinarians");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapGet("/{id:int}/schedule", GetScheduleAsync);
        group.MapGet("/{id:int}/appointments", GetAppointmentsAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IVeterinarianService service,
        string? specialization = null,
        bool? isAvailable = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
        IVeterinarianService service,
        CancellationToken ct = default)
    {
        var vet = await service.GetByIdAsync(id, ct);
        return vet is not null
            ? TypedResults.Ok(vet)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> CreateAsync(
        CreateVeterinarianDto dto,
        IVeterinarianService service,
        CancellationToken ct = default)
    {
        var vet = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateVeterinarianDto dto,
        IVeterinarianService service,
        CancellationToken ct = default)
    {
        var vet = await service.UpdateAsync(id, dto, ct);
        return vet is not null
            ? TypedResults.Ok(vet)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> GetScheduleAsync(
        int id,
        IVeterinarianService service,
        DateOnly? date = null,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = await service.GetScheduleAsync(id, scheduleDate, ct);
        return TypedResults.Ok(schedule);
    }

    private static async Task<IResult> GetAppointmentsAsync(
        int id,
        IVeterinarianService service,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct))
        {
            return TypedResults.NotFound();
        }

        var appointments = await service.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(appointments);
    }
}
