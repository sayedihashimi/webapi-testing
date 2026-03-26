using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class ReviewModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public ReviewModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public List<LeaveBalance> LeaveBalances { get; set; } = new();
    public List<Employee> Reviewers { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Reviewer is required.")]
        [Display(Name = "Reviewer")]
        public int ReviewerId { get; set; }

        [MaxLength(1000, ErrorMessage = "Review notes cannot exceed 1000 characters.")]
        [Display(Name = "Review Notes")]
        public string? ReviewNotes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        LeaveRequest = leaveRequest;
        await LoadPageDataAsync(leaveRequest);

        // Default to employee's manager if available
        if (leaveRequest.Employee.ManagerId.HasValue)
        {
            Input.ReviewerId = leaveRequest.Employee.ManagerId.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            LeaveRequest = leaveRequest;
            await LoadPageDataAsync(leaveRequest);
            return Page();
        }

        try
        {
            await _leaveService.ApproveLeaveRequestAsync(id, Input.ReviewerId, Input.ReviewNotes);
            TempData["SuccessMessage"] = "Leave request approved successfully.";
            return RedirectToPage("/Leave/Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            LeaveRequest = leaveRequest;
            await LoadPageDataAsync(leaveRequest);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            LeaveRequest = leaveRequest;
            await LoadPageDataAsync(leaveRequest);
            return Page();
        }

        try
        {
            await _leaveService.RejectLeaveRequestAsync(id, Input.ReviewerId, Input.ReviewNotes);
            TempData["SuccessMessage"] = "Leave request rejected successfully.";
            return RedirectToPage("/Leave/Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            LeaveRequest = leaveRequest;
            await LoadPageDataAsync(leaveRequest);
            return Page();
        }
    }

    private async Task LoadPageDataAsync(LeaveRequest leaveRequest)
    {
        LeaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(
            leaveRequest.EmployeeId, leaveRequest.StartDate.Year);

        var (employees, _) = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Reviewers = employees;
    }
}
