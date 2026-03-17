using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

// --- Book DTOs ---
public class BookSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int? PublicationYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public List<string> Authors { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

public class BookDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AuthorDto> Authors { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

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

    [Range(1, int.MaxValue)]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
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

    [Range(1, int.MaxValue)]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}
