using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class DetailsModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public DetailsModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public Payment Payment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await _paymentService.GetWithDetailsAsync(id);

        if (payment is null)
        {
            return NotFound();
        }

        Payment = payment;
        return Page();
    }
}
