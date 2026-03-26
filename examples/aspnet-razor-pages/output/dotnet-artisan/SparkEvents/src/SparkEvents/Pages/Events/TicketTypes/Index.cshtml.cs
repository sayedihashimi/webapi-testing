using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Events.TicketTypes;

public sealed class IndexModel(SparkEventsDbContext db) : PageModel
{
    public Event Event { get; set; } = null!;
    public List<TicketType> TicketTypeList { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public sealed class InputModel
    {
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

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        Input.EventId = eventId;
        await LoadTicketTypesAsync(eventId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == Input.EventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;

        if (!ModelState.IsValid)
        {
            await LoadTicketTypesAsync(Input.EventId);
            return Page();
        }

        var ticketType = new TicketType
        {
            EventId = Input.EventId,
            Name = Input.Name,
            Description = Input.Description,
            Price = Input.Price,
            EarlyBirdPrice = Input.EarlyBirdPrice,
            Quantity = Input.Quantity,
            SortOrder = Input.SortOrder,
            IsActive = true,
        };

        db.TicketTypes.Add(ticketType);
        await db.SaveChangesAsync();

        TempData["StatusMessage"] = $"Ticket type '{ticketType.Name}' created.";
        return RedirectToPage(new { eventId = Input.EventId });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id, int eventId)
    {
        var ticketType = await db.TicketTypes.FindAsync(id);
        if (ticketType is null || ticketType.EventId != eventId)
        {
            return NotFound();
        }

        ticketType.IsActive = !ticketType.IsActive;
        await db.SaveChangesAsync();

        var action = ticketType.IsActive ? "activated" : "deactivated";
        TempData["StatusMessage"] = $"Ticket type '{ticketType.Name}' {action}.";
        return RedirectToPage(new { eventId });
    }

    private async Task LoadTicketTypesAsync(int eventId)
    {
        TicketTypeList = await db.TicketTypes
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}
