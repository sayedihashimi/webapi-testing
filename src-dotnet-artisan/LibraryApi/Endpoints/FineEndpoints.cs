using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static RouteGroupBuilder MapFineEndpoints(this RouteGroupBuilder group)
    {
        var fines = group.MapGroup("/fines").WithTags("Fines");

        fines.MapGet("/", GetFinesAsync)
            .WithSummary("List fines with status filter and pagination");

        fines.MapGet("/{id:int}", GetFineByIdAsync)
            .WithSummary("Get fine details");

        fines.MapPost("/{id:int}/pay", PayFineAsync)
            .WithSummary("Pay a fine");

        fines.MapPost("/{id:int}/waive", WaiveFineAsync)
            .WithSummary("Waive a fine");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<FineResponse>>> GetFinesAsync(
        IFineService service, string? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetFinesAsync(status, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<FineDetailResponse>, NotFound>> GetFineByIdAsync(
        int id, IFineService service, CancellationToken ct = default)
    {
        var result = await service.GetFineByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<FineDetailResponse>, NotFound, BadRequest<ProblemDetails>>> PayFineAsync(
        int id, IFineService service, CancellationToken ct = default)
    {
        var result = await service.PayFineAsync(id, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Payment failed",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Results<Ok<FineDetailResponse>, NotFound, BadRequest<ProblemDetails>>> WaiveFineAsync(
        int id, IFineService service, CancellationToken ct = default)
    {
        var result = await service.WaiveFineAsync(id, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Waive failed",
            Detail = result.Error,
            Status = 400
        });
    }
}
