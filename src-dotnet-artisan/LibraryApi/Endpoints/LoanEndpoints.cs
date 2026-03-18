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

        group.MapGet("/", GetLoans).WithName("GetLoans");
        group.MapGet("/overdue", GetOverdueLoans).WithName("GetOverdueLoans");
        group.MapGet("/{id:int}", GetLoan).WithName("GetLoan");
        group.MapPost("/", CheckoutBook).WithName("CheckoutBook");
        group.MapPost("/{id:int}/return", ReturnBook).WithName("ReturnBook");
        group.MapPost("/{id:int}/renew", RenewLoan).WithName("RenewLoan");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<LoanResponse>>> GetLoans(
        LibraryService service,
        LoanStatus? status = null, bool? overdue = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        int page = 1, int pageSize = 10)
    {
        var result = await service.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<LoanResponse>>> GetOverdueLoans(LibraryService service)
    {
        var result = await service.GetOverdueLoansAsync();
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<LoanResponse>, NotFound>> GetLoan(
        LibraryService service, int id)
    {
        var result = await service.GetLoanByIdAsync(id);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<LoanResponse>, BadRequest<string>>> CheckoutBook(
        LibraryService service, CreateLoanRequest request)
    {
        var (result, error) = await service.CheckoutBookAsync(request);
        return result is not null
            ? TypedResults.Created($"/api/loans/{result.Id}", result)
            : TypedResults.BadRequest(error!);
    }

    private static async Task<Results<Ok<LoanResponse>, NotFound, BadRequest<string>>> ReturnBook(
        LibraryService service, int id)
    {
        var (result, error) = await service.ReturnBookAsync(id);
        if (error == "Loan not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(error!);
    }

    private static async Task<Results<Ok<LoanResponse>, NotFound, BadRequest<string>>> RenewLoan(
        LibraryService service, int id)
    {
        var (result, error) = await service.RenewLoanAsync(id);
        if (error == "Loan not found") { return TypedResults.NotFound(); }
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(error!);
    }
}
