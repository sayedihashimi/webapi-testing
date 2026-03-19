using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public sealed record CategoryResponse(int Id, string Name, string? Description);

public sealed record CategoryDetailResponse(
    int Id,
    string Name,
    string? Description,
    int BookCount);

public sealed record CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed record UpdateCategoryRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}
