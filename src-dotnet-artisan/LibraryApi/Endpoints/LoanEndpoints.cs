using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this RouteGroupBuilder group)
    {
        var loans = group.MapGroup("/loans").WithTags("Loans");

        loans.MapGet("/", GetLoansAsync)
            .WithSummary("List loans with status filter, overdue flag, date range, and pagination");

        loans.MapGet("/overdue", GetOverdueLoansAsync)
            .WithSummary("Get all currently overdue loans (also flags active loans past due date)");

        loans.MapGet("/{id:int}", GetLoanByIdAsync)
            .WithSummary("Get loan details");

        loans.MapPost("/", CheckoutBookAsync)
            .WithSummary("Check out a book — creates a loan enforcing all checkout rules");

        loans.MapPost("/{id:int}/return", ReturnBookAsync)
            .WithSummary("Return a book — processes return rules including fines and reservations");

        loans.MapPost("/{id:int}/renew", RenewLoanAsync)
            .WithSummary("Renew a loan — enforces renewal rules");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<LoanResponse>>> GetLoansAsync(
        ILoanService service, string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<LoanDetailResponse>, NotFound>> GetLoanByIdAsync(
        int id, ILoanService service, CancellationToken ct = default)
    {
        var result = await service.GetLoanByIdAsync(id, ct);
        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<Created<LoanDetailResponse>, NotFound, BadRequest<ProblemDetails>>> CheckoutBookAsync(
        CreateLoanRequest request, ILoanService service, CancellationToken ct = default)
    {
        var result = await service.CheckoutBookAsync(request, ct);
        if (result.IsSuccess)
        {
            return TypedResults.Created($"/api/loans/{result.Value!.Id}", result.Value);
        }

        if (result.StatusCode == 404)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Checkout denied",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Results<Ok<LoanDetailResponse>, NotFound, BadRequest<ProblemDetails>>> ReturnBookAsync(
        int id, ILoanService service, CancellationToken ct = default)
    {
        var result = await service.ReturnBookAsync(id, ct);
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
            Title = "Return failed",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Results<Ok<LoanDetailResponse>, NotFound, BadRequest<ProblemDetails>>> RenewLoanAsync(
        int id, ILoanService service, CancellationToken ct = default)
    {
        var result = await service.RenewLoanAsync(id, ct);
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
            Title = "Renewal denied",
            Detail = result.Error,
            Status = 400
        });
    }

    private static async Task<Ok<PaginatedResponse<LoanResponse>>> GetOverdueLoansAsync(
        ILoanService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await service.GetOverdueLoansAsync(page, pageSize, ct);
        return TypedResults.Ok(result);
    }
}
