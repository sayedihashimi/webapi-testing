using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class CreateModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeOptions { get; set; } = [];
    public List<SelectListItem> LeaveTypeOptions { get; set; } = [];

    public class InputModel
    {
        [Required, Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required, Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Required, Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, Display(Name = "End Date")]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        try
        {
            var request = new LeaveRequest
            {
                EmployeeId = Input.EmployeeId,
                LeaveTypeId = Input.LeaveTypeId,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                Reason = Input.Reason
            };

            await leaveService.SubmitRequestAsync(request);
            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var (employees, _) = await employeeService.GetPagedAsync(1, 500, status: EmployeeStatus.Active);
        EmployeeOptions = employees.Select(e => new SelectListItem($"{e.FullName} ({e.EmployeeNumber})", e.Id.ToString())).ToList();

        var types = await leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
    }
}
