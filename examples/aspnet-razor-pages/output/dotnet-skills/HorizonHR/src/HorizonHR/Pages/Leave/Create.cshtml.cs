using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

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

    public List<Employee> Employees { get; set; } = new();
    public List<LeaveType> LeaveTypes { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Leave type is required.")]
        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(int? employeeId)
    {
        await LoadDropdownsAsync();

        if (employeeId.HasValue)
        {
            Input.EmployeeId = employeeId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            var totalDays = await _leaveService.CalculateBusinessDaysAsync(Input.StartDate, Input.EndDate);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = Input.EmployeeId,
                LeaveTypeId = Input.LeaveTypeId,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                TotalDays = totalDays,
                Reason = Input.Reason
            };

            await _leaveService.SubmitLeaveRequestAsync(leaveRequest);

            TempData["SuccessMessage"] = "Leave request submitted successfully.";
            return RedirectToPage("/Leave/Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var (employees, _) = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Employees = employees;
        LeaveTypes = await _leaveService.GetLeaveTypesAsync();
    }
}
