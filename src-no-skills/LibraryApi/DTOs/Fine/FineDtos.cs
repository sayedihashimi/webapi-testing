using LibraryApi.Models.Enums;

namespace LibraryApi.DTOs.Fine;

public record FineListDto(
    int Id, string PatronName, string BookTitle,
    decimal Amount, string Reason, DateTime IssuedDate,
    DateTime? PaidDate, FineStatus Status);

public record FineDetailDto(
    int Id, int PatronId, string PatronName,
    int LoanId, string BookTitle,
    decimal Amount, string Reason, DateTime IssuedDate,
    DateTime? PaidDate, FineStatus Status, DateTime CreatedAt);
