using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;
    public IndexModel(IPaymentService paymentService, ApplicationDbContext context) { _paymentService = paymentService; _context = context; }

    public List<Payment> Payments { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public PaymentType? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public PaymentStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        PropertyList = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        const int pageSize = 10;
        var (items, totalCount) = await _paymentService.GetPaymentsAsync(TypeFilter, StatusFilter, FromDate, ToDate, PropertyId, PageNumber, pageSize, "date", true);
        Payments = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
