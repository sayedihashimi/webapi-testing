using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class OverdueModel : PageModel
{
    private readonly IPaymentService _paymentService;
    public OverdueModel(IPaymentService paymentService) { _paymentService = paymentService; }

    public List<(Lease Lease, DateOnly DueDate)> OverdueItems { get; set; } = new();

    public async Task OnGetAsync()
    {
        OverdueItems = await _paymentService.GetOverduePaymentsAsync();
    }
}
