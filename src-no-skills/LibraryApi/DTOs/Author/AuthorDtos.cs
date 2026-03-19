using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Author;

public record AuthorListDto(int Id, string FirstName, string LastName, string? Country, int BookCount);

public record AuthorDetailDto(
    int Id, string FirstName, string LastName, string? Biography,
    DateOnly? BirthDate, string? Country, DateTime CreatedAt,
    IReadOnlyList<AuthorBookDto> Books);

public record AuthorBookDto(int Id, string Title, string ISBN);

public class CreateAuthorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

public class UpdateAuthorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}
