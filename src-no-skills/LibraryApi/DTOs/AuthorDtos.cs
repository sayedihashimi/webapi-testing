using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthorDetailDto : AuthorDto
{
    public List<BookSummaryDto> Books { get; set; } = new();
}

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
