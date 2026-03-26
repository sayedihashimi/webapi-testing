using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public sealed class DetailsModel(ILeaveService leaveService) : PageModel
{
    public new LeaveRequest Request { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var req = await leaveService.GetRequestByIdAsync(id, ct);
        if (req == null) return NotFound();
        Request = req;
        return Page();
    }
}
