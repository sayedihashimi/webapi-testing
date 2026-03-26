using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public sealed class IndexModel(ILeaveService leaveService) : PageModel
{
    public PaginatedList<LeaveRequest> Requests { get; set; } = null!;
    public LeaveRequestStatus? Status { get; set; }

    public async Task OnGetAsync(LeaveRequestStatus? status, int pageNumber = 1, CancellationToken ct = default)
    {
        Status = status;
        Requests = await leaveService.GetAllRequestsAsync(status, null, null, null, null, pageNumber, 10, ct);
    }
}
