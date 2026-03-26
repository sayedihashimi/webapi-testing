using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events.TicketTypes;

public class EditModel(IEventService eventService, ITicketTypeService ticketTypeService) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public int EventId { get; set; }

    [BindProperty]
    public int TicketTypeId { get; set; }

    [BindProperty]
    public TicketTypeInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId, int id)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        var tt = await ticketTypeService.GetByIdAsync(id);
        if (tt is null || tt.EventId != eventId)
        {
            return NotFound();
        }

        Event = ev;
        EventId = eventId;
        TicketTypeId = id;
        PopulateInput(tt);
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

        var tt = await ticketTypeService.GetByIdAsync(TicketTypeId);
        if (tt is null || tt.EventId != EventId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        tt.Name = Input.Name;
        tt.Description = Input.Description;
        tt.Price = Input.Price;
        tt.EarlyBirdPrice = Input.EarlyBirdPrice;
        tt.Quantity = Input.Quantity;
        tt.SortOrder = Input.SortOrder;

        await ticketTypeService.UpdateAsync(tt);
        TempData["SuccessMessage"] = "Ticket type updated successfully.";
        return RedirectToPage("Index", new { eventId = EventId });
    }

    private void PopulateInput(TicketType tt)
    {
        Input = new TicketTypeInputModel
        {
            Name = tt.Name,
            Description = tt.Description,
            Price = tt.Price,
            EarlyBirdPrice = tt.EarlyBirdPrice,
            Quantity = tt.Quantity,
            SortOrder = tt.SortOrder
        };
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
