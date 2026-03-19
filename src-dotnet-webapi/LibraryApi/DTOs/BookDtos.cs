using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public sealed record BookResponse(
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
    DateTime UpdatedAt);

public sealed record BookDetailResponse(
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
    IReadOnlyList<BookAuthorResponse> Authors,
    IReadOnlyList<BookCategoryResponse> Categories);

public sealed record BookAuthorResponse(int Id, string FirstName, string LastName);
public sealed record BookCategoryResponse(int Id, string Name);

public sealed record CreateBookRequest
{
    [Required, MaxLength(300)]
    public required string Title { get; init; }

    [Required, MaxLength(20)]
    public required string ISBN { get; init; }

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(50)]
    public string? Language { get; init; }

    [Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }

    public required IReadOnlyList<int> AuthorIds { get; init; }
    public required IReadOnlyList<int> CategoryIds { get; init; }
}

public sealed record UpdateBookRequest
{
    [Required, MaxLength(300)]
    public required string Title { get; init; }

    [Required, MaxLength(20)]
    public required string ISBN { get; init; }

    [MaxLength(200)]
    public string? Publisher { get; init; }

    public int? PublicationYear { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageCount { get; init; }

    [MaxLength(50)]
    public string? Language { get; init; }

    [Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }

    public required IReadOnlyList<int> AuthorIds { get; init; }
    public required IReadOnlyList<int> CategoryIds { get; init; }
}
