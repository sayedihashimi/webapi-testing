using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class CreateModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Employees { get; set; } = [];
    public List<SelectListItem> LeaveTypes { get; set; } = [];

    public class InputModel
    {
        [Required] public int EmployeeId { get; set; }
        [Required] public int LeaveTypeId { get; set; }
        [Required] public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required] public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await LoadSelectLists();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadSelectLists(); return Page(); }

        if (Input.EndDate < Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be on or after start date.");
            await LoadSelectLists();
            return Page();
        }

        var totalDays = await leaveService.CalculateBusinessDays(Input.StartDate, Input.EndDate);
        var request = new LeaveRequest
        {
            EmployeeId = Input.EmployeeId,
            LeaveTypeId = Input.LeaveTypeId,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            TotalDays = totalDays,
            Reason = Input.Reason
        };

        var error = await leaveService.SubmitRequestAsync(request);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            await LoadSelectLists();
            return Page();
        }

        TempData["Success"] = "Leave request submitted successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadSelectLists()
    {
        var emps = await employeeService.GetAllActiveAsync();
        Employees = emps.Select(e => new SelectListItem($"{e.FullName} ({e.EmployeeNumber})", e.Id.ToString())).ToList();
        var types = await leaveService.GetLeaveTypesAsync();
        LeaveTypes = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
    }
}
