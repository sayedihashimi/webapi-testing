using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public sealed class DetailsModel(IPaymentService paymentService) : PageModel
{
    public Payment Payment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var payment = await paymentService.GetByIdAsync(id, ct);

        if (payment is null)
        {
            return NotFound();
        }

        Payment = payment;
        return Page();
    }
}
