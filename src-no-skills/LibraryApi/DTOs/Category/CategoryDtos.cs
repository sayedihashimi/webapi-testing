using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Category;

public record CategoryListDto(int Id, string Name, string? Description, int BookCount);

public record CategoryDetailDto(int Id, string Name, string? Description, int BookCount);

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
