using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public sealed class IndexModel(IPaymentService paymentService, IUnitService unitService) : PageModel
{
    public PaginatedList<Payment> Payments { get; set; } = null!;
    public List<Property> Properties { get; set; } = [];

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

    public async Task OnGetAsync(CancellationToken ct)
    {
        Properties = await unitService.GetAllPropertiesAsync(ct);
        Payments = await paymentService.GetAllAsync(Type, Status, FromDate, ToDate, PropertyId, PageNumber, 20, ct);
    }
}
