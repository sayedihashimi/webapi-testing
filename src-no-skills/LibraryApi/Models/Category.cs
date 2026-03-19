using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
