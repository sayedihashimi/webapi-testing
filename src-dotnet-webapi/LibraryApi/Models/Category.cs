namespace LibraryApi.Models;

public sealed class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<BookCategory> BookCategories { get; set; } = [];
}
