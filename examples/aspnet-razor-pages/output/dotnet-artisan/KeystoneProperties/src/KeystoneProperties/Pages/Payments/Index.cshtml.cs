using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public sealed class IndexModel(
    IPaymentService paymentService,
    IPropertyService propertyService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public PaymentType? PaymentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentStatus? PaymentStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "date_desc";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PaginatedList<Payment> Payments { get; set; } = null!;
    public SelectList Properties { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var properties = await propertyService.GetPropertiesAsync(null, null, true, 1, 100);
        Properties = new SelectList(properties.Items, "Id", "Name");

        Payments = await paymentService.GetPaymentsAsync(
            PaymentType, PaymentStatus, FromDate, ToDate, PropertyId, PageNumber, 10);
    }

    public string BuildFilterUrl()
    {
        var parts = new List<string> { "?handler=get" };

        if (PaymentType.HasValue)
            parts.Add($"paymentType={PaymentType}");
        if (PaymentStatus.HasValue)
            parts.Add($"paymentStatus={PaymentStatus}");
        if (FromDate.HasValue)
            parts.Add($"fromDate={FromDate:yyyy-MM-dd}");
        if (ToDate.HasValue)
            parts.Add($"toDate={ToDate:yyyy-MM-dd}");
        if (PropertyId.HasValue)
            parts.Add($"propertyId={PropertyId}");
        if (!string.IsNullOrEmpty(SortOrder))
            parts.Add($"sortOrder={SortOrder}");

        return "/Payments/Index?" + string.Join("&", parts.Skip(1));
    }
}
