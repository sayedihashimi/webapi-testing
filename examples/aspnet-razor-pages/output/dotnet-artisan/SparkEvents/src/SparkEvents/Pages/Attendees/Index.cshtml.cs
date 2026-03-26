using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Attendees;

public sealed class IndexModel(SparkEventsDbContext db) : PageModel
{
    public IReadOnlyList<AttendeeRow> Attendees { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        const int pageSize = 10;
        PageNumber = pageNumber;

        var query = db.Attendees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(a =>
                a.FirstName.Contains(term) ||
                a.LastName.Contains(term) ||
                a.Email.Contains(term));
        }

        TotalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);

        Attendees = await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Skip((PageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendeeRow
            {
                Id = a.Id,
                FullName = a.FirstName + " " + a.LastName,
                Email = a.Email,
                Organization = a.Organization,
                RegistrationCount = a.Registrations.Count
            })
            .ToListAsync();
    }

    public sealed class AttendeeRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public int RegistrationCount { get; set; }
    }
}
