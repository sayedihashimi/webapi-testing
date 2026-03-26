using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events.TicketTypes;

public class CreateModel(IEventService eventService, ITicketTypeService ticketTypeService) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public int EventId { get; set; }

    [BindProperty]
    public TicketTypeInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        EventId = eventId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ev = await eventService.GetByIdAsync(EventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ticketType = new TicketType
        {
            EventId = EventId,
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            EarlyBirdPrice = Input.EarlyBirdPrice,
            Quantity = Input.Quantity,
            SortOrder = Input.SortOrder
        };

        await ticketTypeService.CreateAsync(ticketType);
        TempData["SuccessMessage"] = "Ticket type created successfully.";
        return RedirectToPage("Index", new { eventId = EventId });
    }

    public class TicketTypeInputModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Early bird price must be zero or greater.")]
        [Display(Name = "Early Bird Price")]
        public decimal? EarlyBirdPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}
