namespace LibraryApi.Models;

public class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BookAuthor> BookAuthors { get; set; } = [];
}
