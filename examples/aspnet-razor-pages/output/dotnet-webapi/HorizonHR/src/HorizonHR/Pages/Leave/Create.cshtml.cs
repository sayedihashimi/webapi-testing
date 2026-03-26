using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public sealed class CreateModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeList { get; set; } = [];
    public List<SelectListItem> LeaveTypeList { get; set; } = [];

    public sealed class InputModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(ct);
            return Page();
        }

        try
        {
            var request = await leaveService.SubmitRequestAsync(Input.EmployeeId, Input.LeaveTypeId, Input.StartDate, Input.EndDate, Input.Reason, ct);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToPage("Details", new { id = request.Id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync(ct);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        var employees = await employeeService.GetAllAsync(null, null, null, EmployeeStatus.Active, 1, 1000, ct);
        EmployeeList = employees.Items.Select(e => new SelectListItem($"{e.FirstName} {e.LastName} ({e.EmployeeNumber})", e.Id.ToString())).ToList();

        var leaveTypes = await leaveService.GetLeaveTypesAsync(ct);
        LeaveTypeList = leaveTypes.Select(lt => new SelectListItem(lt.Name, lt.Id.ToString())).ToList();
    }
}
