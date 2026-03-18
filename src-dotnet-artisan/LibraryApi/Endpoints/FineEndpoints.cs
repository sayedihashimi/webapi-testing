using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", GetFines).WithName("GetFines");
        group.MapGet("/{id:int}", GetFine).WithName("GetFine");
        group.MapPost("/{id:int}/pay", PayFine).WithName("PayFine");
        group.MapPost("/{id:int}/waive", WaiveFine).WithName("WaiveFine");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<FineResponse>>> GetFines(
        LibraryService service, FineStatus? status = null, int page = 1, int pageSize = 10)
    {
        var result = await service.GetFinesAsync(status, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<FineResponse>, NotFound>> GetFine(
        LibraryService service, int id)
    {
        var result = await service.GetFineByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<FineResponse>, NotFound, BadRequest<string>>> PayFine(
        LibraryService service, int id)
    {
        var (result, error) = await service.PayFineAsync(id);
        if (error == "Fine not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(error!);
    }

    private static async Task<Results<Ok<FineResponse>, NotFound, BadRequest<string>>> WaiveFine(
        LibraryService service, int id)
    {
        var (result, error) = await service.WaiveFineAsync(id);
        if (error == "Fine not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(error!);
    }
}
