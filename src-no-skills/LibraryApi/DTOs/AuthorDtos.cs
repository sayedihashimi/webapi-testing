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
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
}

public class UpdateAuthorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
}
