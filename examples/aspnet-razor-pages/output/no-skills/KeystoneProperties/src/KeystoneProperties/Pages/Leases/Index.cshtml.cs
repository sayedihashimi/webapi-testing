using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Leases;

public class IndexModel : PageModel
{
    private readonly ILeaseService _leaseService;
    private readonly ApplicationDbContext _context;
    public IndexModel(ILeaseService leaseService, ApplicationDbContext context) { _leaseService = leaseService; _context = context; }

    public List<Lease> Leases { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public LeaseStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        PropertyList = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        const int pageSize = 10;
        var (items, totalCount) = await _leaseService.GetLeasesAsync(StatusFilter, PropertyId, PageNumber, pageSize);
        Leases = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
