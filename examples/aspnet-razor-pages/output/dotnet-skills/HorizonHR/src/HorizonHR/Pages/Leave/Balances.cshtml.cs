using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class BalancesModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IDepartmentService _departmentService;

    public BalancesModel(ILeaveService leaveService, IDepartmentService departmentService)
    {
        _leaveService = leaveService;
        _departmentService = departmentService;
    }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [BindProperty(SupportsGet = true)]
    public int pageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int pageSize { get; set; } = 10;

    public List<LeaveBalance> LeaveBalances { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();

        var (items, totalCount) = await _leaveService.GetAllLeaveBalancesAsync(
            DepartmentId, Year, pageNumber, pageSize);

        LeaveBalances = items;

        Pagination = new PaginationModel
        {
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            PageUrl = "/Leave/Balances"
        };
    }
}
