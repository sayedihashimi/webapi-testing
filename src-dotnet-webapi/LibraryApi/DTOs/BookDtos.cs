using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

public class CreateBookRequest
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^978-\d{10}$", ErrorMessage = "ISBN must be in format 978-XXXXXXXXXX")]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page count must be positive")]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
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
    [RegularExpression(@"^978-\d{10}$", ErrorMessage = "ISBN must be in format 978-XXXXXXXXXX")]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page count must be positive")]
    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
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
    public DateTime? UpdatedAt { get; set; }
    public List<BookAuthorResponse> Authors { get; set; } = [];
    public List<BookCategoryResponse> Categories { get; set; } = [];
}

public class BookAuthorResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class BookCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
