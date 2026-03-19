using LibraryApi.DTOs;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct);
    Task<LoanDetailResponse?> GetLoanByIdAsync(int id, CancellationToken ct);
    Task<Result<LoanDetailResponse>> CheckoutBookAsync(CreateLoanRequest request, CancellationToken ct);
    Task<Result<LoanDetailResponse>> ReturnBookAsync(int loanId, CancellationToken ct);
    Task<Result<LoanDetailResponse>> RenewLoanAsync(int loanId, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize, CancellationToken ct);
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    private Result(T value) { IsSuccess = true; Value = value; StatusCode = 200; }
    private Result(string error, int statusCode) { IsSuccess = false; Error = error; StatusCode = statusCode; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, int statusCode = 400) => new(error, statusCode);
}
