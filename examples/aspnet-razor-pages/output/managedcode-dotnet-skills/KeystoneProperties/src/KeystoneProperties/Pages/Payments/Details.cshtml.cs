using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Payments;

public class DetailsModel(IPaymentService paymentService) : PageModel
{
    public Payment Payment { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await paymentService.GetPaymentWithDetailsAsync(id);

        if (payment is null)
        {
            return NotFound();
        }

        Payment = payment;
        return Page();
    }
}
