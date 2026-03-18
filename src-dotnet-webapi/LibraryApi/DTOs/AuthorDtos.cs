using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class CreateAuthorRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

public class UpdateAuthorRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

public class AuthorResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthorDetailResponse : AuthorResponse
{
    public List<AuthorBookResponse> Books { get; set; } = [];
}

public class AuthorBookResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
}

public class AuthorSummaryResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
