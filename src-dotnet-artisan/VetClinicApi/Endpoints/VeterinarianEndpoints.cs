using Microsoft.AspNetCore.Http.HttpResults;
using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", GetAll)
            .WithName("GetVeterinarians")
            .WithSummary("Get all veterinarians with optional filters");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetVeterinarianById")
            .WithSummary("Get veterinarian by ID");

        group.MapPost("/", Create)
            .WithName("CreateVeterinarian")
            .WithSummary("Create a new veterinarian");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateVeterinarian")
            .WithSummary("Update an existing veterinarian");

        group.MapGet("/{id:int}/schedule", GetSchedule)
            .WithName("GetVeterinarianSchedule")
            .WithSummary("Get a veterinarian's schedule for a specific date");

        group.MapGet("/{id:int}/appointments", GetAppointments)
            .WithName("GetVeterinarianAppointments")
            .WithSummary("Get appointments for a veterinarian with optional status filter");

        return group;
    }

    private static async Task<Ok<PagedResponse<VeterinarianResponse>>> GetAll(
        VeterinarianService service, string? specialization = null, bool? isAvailable = null,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<VeterinarianResponse>, NotFound>> GetById(
        int id, VeterinarianService service, CancellationToken ct)
    {
        var vet = await service.GetByIdAsync(id, ct);
        return vet is not null ? TypedResults.Ok(vet) : TypedResults.NotFound();
    }

    private static async Task<Created<VeterinarianResponse>> Create(
        CreateVeterinarianRequest request, VeterinarianService service, CancellationToken ct)
    {
        var vet = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
    }

    private static async Task<Results<Ok<VeterinarianResponse>, NotFound>> Update(
        int id, UpdateVeterinarianRequest request, VeterinarianService service, CancellationToken ct)
    {
        var vet = await service.UpdateAsync(id, request, ct);
        return vet is not null ? TypedResults.Ok(vet) : TypedResults.NotFound();
    }

    private static async Task<Ok<List<AppointmentResponse>>> GetSchedule(
        int id, VeterinarianService service, DateOnly? date = null, CancellationToken ct = default)
    {
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = await service.GetScheduleAsync(id, scheduleDate, ct);
        return TypedResults.Ok(schedule);
    }

    private static async Task<Ok<PagedResponse<AppointmentResponse>>> GetAppointments(
        int id, VeterinarianService service, AppointmentStatus? status = null,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var result = await service.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}
