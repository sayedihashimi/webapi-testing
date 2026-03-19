using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/", async Task<Ok<PaginatedResponse<LoanResponse>>> (
            LoanStatus? status, bool? overdue,
            DateTime? fromDate, DateTime? toDate,
            int? page, int? pageSize,
            ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(
                status, overdue, fromDate, toDate,
                page ?? 1, Math.Min(pageSize ?? 20, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("Returns a paginated list of loans. Filter by status, overdue flag, or date range.");

        group.MapGet("/overdue", async Task<Ok<IReadOnlyList<LoanResponse>>> (
            ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetOverdueAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get all overdue loans")
        .WithDescription("Returns all overdue loans. Also updates active loans past their due date to overdue status.");

        group.MapGet("/{id:int}", async Task<Results<Ok<LoanDetailResponse>, NotFound>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan by ID");

        group.MapPost("/", async Task<Created<LoanResponse>> (
            CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.CheckoutAsync(request, ct);
            return TypedResults.Created($"/api/loans/{result.Id}", result);
        })
        .WithName("CheckoutBook")
        .WithSummary("Checkout a book")
        .WithDescription("Creates a new loan. Enforces borrowing limits, fine thresholds, and availability.");

        group.MapPost("/{id:int}/return", async Task<Ok<LoanResponse>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.ReturnAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("ReturnBook")
        .WithSummary("Return a book")
        .WithDescription("Processes a return. Generates fines for overdue items and updates reservation queue.");

        group.MapPost("/{id:int}/renew", async Task<Ok<LoanResponse>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.RenewAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Extends loan due date. Max 2 renewals. Blocked by pending reservations or high fines.");

        return group;
    }
}
