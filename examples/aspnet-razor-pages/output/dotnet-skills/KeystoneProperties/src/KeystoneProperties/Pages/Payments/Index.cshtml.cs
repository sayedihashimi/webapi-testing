using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IPaymentService paymentService, IPropertyService propertyService)
    {
        _paymentService = paymentService;
        _propertyService = propertyService;
    }

    public PaginatedList<Payment> Payments { get; set; } = null!;
    public SelectList PropertyList { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public PaymentType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");

        Payments = await _paymentService.GetPaymentsAsync(
            Type, Status, FromDate, ToDate, PropertyId, PageNumber, 10);
    }
}
