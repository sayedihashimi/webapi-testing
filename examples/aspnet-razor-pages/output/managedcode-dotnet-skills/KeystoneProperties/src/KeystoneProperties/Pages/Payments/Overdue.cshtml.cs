using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class OverdueModel(IPaymentService paymentService) : PageModel
{
    public List<LeasePaymentInfo> OverduePayments { get; set; } = [];

    public async Task OnGetAsync()
    {
        OverduePayments = await paymentService.GetOverduePaymentsAsync();
    }
}
