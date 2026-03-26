using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class OverdueModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public OverdueModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public List<OverduePaymentInfo> OverduePayments { get; set; } = new();

    public async Task OnGetAsync()
    {
        OverduePayments = await _paymentService.GetOverduePaymentsAsync();
    }
}
