using LibraryApi.Models;

namespace LibraryApi.DTOs;

// Author DTOs
public sealed record AuthorSummaryDto(int Id, string FirstName, string LastName, string? Country);

public sealed record AuthorDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    List<BookSummaryDto> Books);

public sealed record CreateAuthorDto(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country);

public sealed record UpdateAuthorDto(
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country);

// Category DTOs
public sealed record CategoryDto(int Id, string Name, string? Description);

public sealed record CategoryDetailDto(int Id, string Name, string? Description, int BookCount);

public sealed record CreateCategoryDto(string Name, string? Description);

public sealed record UpdateCategoryDto(string Name, string? Description);

// Book DTOs
public sealed record BookSummaryDto(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string Language,
    int TotalCopies,
    int AvailableCopies);

public sealed record BookDetailDto(
    int Id,
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string Language,
    int TotalCopies,
    int AvailableCopies,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<AuthorSummaryDto> Authors,
    List<CategoryDto> Categories);

public sealed record CreateBookDto(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds);

public sealed record UpdateBookDto(
    string Title,
    string ISBN,
    string? Publisher,
    int? PublicationYear,
    string? Description,
    int? PageCount,
    string? Language,
    int TotalCopies,
    List<int> AuthorIds,
    List<int> CategoryIds);

// Patron DTOs
public sealed record PatronSummaryDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    MembershipType MembershipType,
    bool IsActive);

public sealed record PatronDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateOnly MembershipDate,
    MembershipType MembershipType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ActiveLoansCount,
    decimal UnpaidFinesBalance);

public sealed record CreatePatronDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType);

public sealed record UpdatePatronDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType);

// Loan DTOs
public sealed record LoanDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount,
    DateTime CreatedAt);

public sealed record CreateLoanDto(int BookId, int PatronId);

public sealed record LoanDetailDto(
    int Id,
    int BookId,
    string BookTitle,
    string BookISBN,
    int PatronId,
    string PatronName,
    string PatronEmail,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    LoanStatus Status,
    int RenewalCount,
    DateTime CreatedAt);

// Reservation DTOs
public sealed record ReservationDto(
    int Id,
    int BookId,
    string BookTitle,
    int PatronId,
    string PatronName,
    DateTime ReservationDate,
    DateTime? ExpirationDate,
    ReservationStatus Status,
    int QueuePosition,
    DateTime CreatedAt);

public sealed record CreateReservationDto(int BookId, int PatronId);

// Fine DTOs
public sealed record FineDto(
    int Id,
    int PatronId,
    string PatronName,
    int LoanId,
    string BookTitle,
    decimal Amount,
    string Reason,
    DateTime IssuedDate,
    DateTime? PaidDate,
    FineStatus Status,
    DateTime CreatedAt);

// Pagination
public sealed record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
