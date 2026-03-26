using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Events.TicketTypes;

public sealed class EditModel(SparkEventsDbContext db) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be >= 0.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Early Bird Price")]
        public decimal? EarlyBirdPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId, int id)
    {
        var ticketType = await db.TicketTypes.FirstOrDefaultAsync(t => t.Id == id && t.EventId == eventId);
        if (ticketType is null)
        {
            return NotFound();
        }

        var evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        Input = new InputModel
        {
            Id = ticketType.Id,
            EventId = ticketType.EventId,
            Name = ticketType.Name,
            Description = ticketType.Description,
            Price = ticketType.Price,
            EarlyBirdPrice = ticketType.EarlyBirdPrice,
            Quantity = ticketType.Quantity,
            SortOrder = ticketType.SortOrder,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ticketType = await db.TicketTypes.FindAsync(Input.Id);
        if (ticketType is null || ticketType.EventId != Input.EventId)
        {
            return NotFound();
        }

        var evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == Input.EventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;

        if (Input.Quantity < ticketType.QuantitySold)
        {
            ModelState.AddModelError("Input.Quantity", $"Quantity cannot be less than tickets already sold ({ticketType.QuantitySold}).");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ticketType.Name = Input.Name;
        ticketType.Description = Input.Description;
        ticketType.Price = Input.Price;
        ticketType.EarlyBirdPrice = Input.EarlyBirdPrice;
        ticketType.Quantity = Input.Quantity;
        ticketType.SortOrder = Input.SortOrder;

        await db.SaveChangesAsync();

        TempData["StatusMessage"] = $"Ticket type '{ticketType.Name}' updated.";
        return RedirectToPage("Index", new { eventId = Input.EventId });
    }
}
