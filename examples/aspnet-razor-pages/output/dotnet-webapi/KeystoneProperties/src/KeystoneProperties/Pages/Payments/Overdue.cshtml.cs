using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public sealed class OverdueModel(IPaymentService paymentService) : PageModel
{
    public List<Lease> OverdueLeases { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        OverdueLeases = await paymentService.GetOverdueLeasePaymentsAsync(ct);
    }
}
