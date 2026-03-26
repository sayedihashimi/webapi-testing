using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public sealed class DetailsModel(IPaymentService paymentService) : PageModel
{
    public Payment Payment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await paymentService.GetByIdAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        Payment = payment;
        return Page();
    }
}
