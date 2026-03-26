using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Properties;

public sealed class IndexModel(IPropertyService propertyService) : PageModel
{
    public PaginatedList<Property> Properties { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public PropertyType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public SelectList PropertyTypeOptions { get; } = new(
        Enum.GetValues<PropertyType>().Select(t => new { Value = t.ToString(), Text = t.ToString() }),
        "Value", "Text");

    public Dictionary<int, int> OccupiedCounts { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;

        Properties = await propertyService.GetAllAsync(Search, Type, IsActive, PageNumber, PageSize, ct);

        foreach (var property in Properties.Items)
        {
            OccupiedCounts[property.Id] = await propertyService.GetOccupiedUnitCountAsync(property.Id, ct);
        }
    }
}
