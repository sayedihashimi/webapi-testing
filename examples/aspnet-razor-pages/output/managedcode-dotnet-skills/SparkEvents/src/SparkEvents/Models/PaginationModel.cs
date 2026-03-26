namespace SparkEvents.Models;

public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public required Func<int, string> PageUrl { get; set; }
}
