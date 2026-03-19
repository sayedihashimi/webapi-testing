using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class BookCreateDto
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

    [Range(1, int.MaxValue, ErrorMessage = "PageCount must be positive")]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be at least 1")]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}

public class BookUpdateDto
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

    [Range(1, int.MaxValue, ErrorMessage = "PageCount must be positive")]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be at least 1")]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? Description { get; set; }
    public int? PageCount { get; set; }
    public string Language { get; set; } = "English";
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AuthorSummaryDto> Authors { get; set; } = new();
    public List<CategorySummaryDto> Categories { get; set; } = new();
}

public class BookSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class BookDetailDto : BookDto
{
}
