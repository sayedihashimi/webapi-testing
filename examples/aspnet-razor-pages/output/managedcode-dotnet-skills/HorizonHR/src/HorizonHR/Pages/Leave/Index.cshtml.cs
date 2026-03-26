using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class IndexModel(ILeaveService leaveService) : PageModel
{
    public List<LeaveRequest> Requests { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public LeaveRequestStatus? StatusFilter { get; set; }
    public int? LeaveTypeId { get; set; }
    public List<SelectListItem> LeaveTypeOptions { get; set; } = [];

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10,
        LeaveRequestStatus? status = null, int? leaveTypeId = null)
    {
        PageNumber = pageNumber;
        StatusFilter = status;
        LeaveTypeId = leaveTypeId;

        var (items, total) = await leaveService.GetPagedRequestsAsync(
            pageNumber, pageSize, status, leaveTypeId: leaveTypeId);
        Requests = items;
        TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        var types = await leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
    }
}
