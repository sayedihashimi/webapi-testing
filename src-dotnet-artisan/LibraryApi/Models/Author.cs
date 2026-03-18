using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public sealed class Author
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookAuthor> BookAuthors { get; set; } = [];
}
