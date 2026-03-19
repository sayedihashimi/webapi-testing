using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs.Book;

public record BookListDto(
    int Id, string Title, string ISBN, string? Publisher,
    int? PublicationYear, string Language, int TotalCopies,
    int AvailableCopies, IReadOnlyList<string> Authors, IReadOnlyList<string> Categories);

public record BookDetailDto(
    int Id, string Title, string ISBN, string? Publisher,
    int? PublicationYear, string? Description, int? PageCount,
    string Language, int TotalCopies, int AvailableCopies,
    DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<BookAuthorDto> Authors, IReadOnlyList<BookCategoryDto> Categories);

public record BookAuthorDto(int Id, string FirstName, string LastName);
public record BookCategoryDto(int Id, string Name);

public class CreateBookDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string Language { get; set; } = "English";

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; } = 1;

    public List<int> AuthorIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}

public class UpdateBookDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string Language { get; set; } = "English";

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; } = 1;

    public List<int> AuthorIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}
