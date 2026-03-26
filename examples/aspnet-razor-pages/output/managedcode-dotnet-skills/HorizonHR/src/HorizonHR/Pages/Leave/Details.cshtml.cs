using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class DetailsModel(ILeaveService leaveService) : PageModel
{
    public new LeaveRequest? Request { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Request = await leaveService.GetRequestByIdAsync(id);
        if (Request is null) return NotFound();
        return Page();
    }
}
