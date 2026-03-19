using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public sealed record CategoryResponse(
    int Id,
    string Name,
    string? Description);

public sealed record CategoryDetailResponse(
    int Id,
    string Name,
    string? Description,
    int BookCount);

public sealed class CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed class UpdateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
