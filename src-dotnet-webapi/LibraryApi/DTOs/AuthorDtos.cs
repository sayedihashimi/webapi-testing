using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

// Requests
public sealed record CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [MaxLength(2000)]
    public string? Biography { get; init; }

    public DateOnly? BirthDate { get; init; }

    [MaxLength(100)]
    public string? Country { get; init; }
}

public sealed record UpdateAuthorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [MaxLength(2000)]
    public string? Biography { get; init; }

    public DateOnly? BirthDate { get; init; }

    [MaxLength(100)]
    public string? Country { get; init; }
}

// Responses
public sealed record AuthorResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt);

public sealed record AuthorDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateOnly? BirthDate,
    string? Country,
    DateTime CreatedAt,
    IReadOnlyList<AuthorBookResponse> Books);

public sealed record AuthorBookResponse(
    int Id,
    string Title,
    string ISBN);
