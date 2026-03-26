using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class IndexModel(IPaymentService paymentService, IUnitService unitService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public PaymentType? PaymentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentStatus? PaymentStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PaginatedList<Payment> Payments { get; set; } = default!;

    public List<Property> AllProperties { get; set; } = [];

    public async Task OnGetAsync()
    {
        AllProperties = await unitService.GetAllPropertiesAsync();
        Payments = await paymentService.GetPaymentsAsync(
            PaymentType, PaymentStatus, StartDate, EndDate, PropertyId, PageNumber, 10);

        ViewData["PageIndex"] = Payments.PageIndex;
        ViewData["TotalPages"] = Payments.TotalPages;

        var queryParams = new List<string>();
        if (PaymentType.HasValue) queryParams.Add($"paymentType={PaymentType}");
        if (PaymentStatus.HasValue) queryParams.Add($"paymentStatus={PaymentStatus}");
        if (StartDate.HasValue) queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
        if (EndDate.HasValue) queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
        if (PropertyId.HasValue) queryParams.Add($"propertyId={PropertyId}");

        ViewData["PageUrl"] = "/Payments?" + string.Join("&", queryParams);
    }
}
