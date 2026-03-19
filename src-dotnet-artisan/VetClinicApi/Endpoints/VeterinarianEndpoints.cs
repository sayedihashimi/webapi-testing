using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this RouteGroupBuilder group)
    {
        var vets = group.MapGroup("/veterinarians").WithTags("Veterinarians");

        vets.MapGet("/", GetVeterinarians).WithSummary("List all veterinarians with optional filters");
        vets.MapGet("/{id:int}", GetVeterinarianById).WithSummary("Get veterinarian details");
        vets.MapPost("/", CreateVeterinarian).WithSummary("Create a new veterinarian");
        vets.MapPut("/{id:int}", UpdateVeterinarian).WithSummary("Update veterinarian info");
        vets.MapGet("/{id:int}/schedule", GetVetSchedule).WithSummary("Get vet's appointments for a specific date");
        vets.MapGet("/{id:int}/appointments", GetVetAppointments).WithSummary("Get all appointments for a vet with pagination and status filter");

        return group;
    }

    private static async Task<Ok<PagedResult<VeterinarianResponse>>> GetVeterinarians(
        IVeterinarianService service,
        string? specialization = null,
        bool? isAvailable = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAllAsync(specialization, isAvailable, page, pageSize, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<VeterinarianResponse>, NotFound>> GetVeterinarianById(
        int id,
        IVeterinarianService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Created<VeterinarianResponse>> CreateVeterinarian(
        CreateVeterinarianRequest request,
        IVeterinarianService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/veterinarians/{result.Id}", result);
    }

    private static async Task<Results<Ok<VeterinarianResponse>, NotFound>> UpdateVeterinarian(
        int id,
        UpdateVeterinarianRequest request,
        IVeterinarianService service,
        CancellationToken cancellationToken = default)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<AppointmentResponse>>> GetVetSchedule(
        int id,
        IVeterinarianService service,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.GetScheduleAsync(id, scheduleDate, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PagedResult<AppointmentResponse>>> GetVetAppointments(
        int id,
        IVeterinarianService service,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetAppointmentsAsync(id, status, page, pageSize, cancellationToken);
        return TypedResults.Ok(result);
    }
}
