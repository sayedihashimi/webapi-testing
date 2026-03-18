using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class CreateBookRequest
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
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

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}

public class UpdateBookRequest
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
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

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    public List<int> AuthorIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}

public class BookResponse
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
    public List<AuthorSummaryResponse> Authors { get; set; } = [];
    public List<CategorySummaryResponse> Categories { get; set; } = [];
}

public class BookSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
}
