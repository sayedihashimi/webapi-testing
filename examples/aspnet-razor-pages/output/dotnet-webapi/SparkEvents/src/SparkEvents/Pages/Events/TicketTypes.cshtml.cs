using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class TicketTypesModel : PageModel
{
    private readonly ITicketTypeService _ticketTypeService;
    private readonly IEventService _eventService;

    public TicketTypesModel(ITicketTypeService ticketTypeService, IEventService eventService)
    {
        _ticketTypeService = ticketTypeService;
        _eventService = eventService;
    }

    public Event Event { get; set; } = null!;
    public List<TicketType> TicketTypesList { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Early Bird Price")]
        public decimal? EarlyBirdPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int Quantity { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdAsync(eventId, ct);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        TicketTypesList = await _ticketTypeService.GetByEventIdAsync(eventId, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int eventId, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdAsync(eventId, ct);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        TicketTypesList = await _ticketTypeService.GetByEventIdAsync(eventId, ct);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ticketType = new TicketType
        {
            EventId = eventId,
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            EarlyBirdPrice = Input.EarlyBirdPrice,
            Quantity = Input.Quantity,
            SortOrder = Input.SortOrder
        };

        await _ticketTypeService.CreateAsync(ticketType, ct);

        TempData["SuccessMessage"] = $"Ticket type '{Input.Name}' created successfully!";
        return RedirectToPage(new { eventId });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int eventId, int ticketTypeId, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdAsync(eventId, ct);
        if (evt is null)
        {
            return NotFound();
        }

        var success = await _ticketTypeService.ToggleActiveAsync(ticketTypeId, ct);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to toggle ticket type status.";
        }
        else
        {
            TempData["SuccessMessage"] = "Ticket type status updated.";
        }

        return RedirectToPage(new { eventId });
    }
}
