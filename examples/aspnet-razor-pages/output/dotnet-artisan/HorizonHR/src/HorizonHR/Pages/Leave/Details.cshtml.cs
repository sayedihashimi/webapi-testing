using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class DetailsModel(ILeaveService leaveService) : PageModel
{
    public LeaveRequest Request { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var req = await leaveService.GetRequestByIdAsync(id);
        if (req is null) { return NotFound(); }
        Request = req;
        return Page();
    }
}
