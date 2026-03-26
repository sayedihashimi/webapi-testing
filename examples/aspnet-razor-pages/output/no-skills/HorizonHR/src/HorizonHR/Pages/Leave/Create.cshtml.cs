using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class CreateModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public CreateModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> EmployeeOptions { get; set; } = new();
    public List<SelectListItem> LeaveTypeOptions { get; set; } = new();

    public class InputModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, Range(0.5, 365)]
        public decimal TotalDays { get; set; } = 1;

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

        if (Input.EndDate < Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be on or after start date.");
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
                TotalDays = Input.TotalDays,
                Reason = Input.Reason
            };

            await _leaveService.SubmitRequestAsync(request);
            TempData["Success"] = "Leave request submitted successfully.";
            return RedirectToPage("Details", new { id = request.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();

        var leaveTypes = await _leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = leaveTypes.Select(lt => new SelectListItem(lt.Name, lt.Id.ToString())).ToList();
    }
}
