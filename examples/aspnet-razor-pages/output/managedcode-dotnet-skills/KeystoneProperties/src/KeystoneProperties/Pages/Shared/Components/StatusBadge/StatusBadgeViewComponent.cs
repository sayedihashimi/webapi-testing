using Microsoft.AspNetCore.Mvc;

namespace KeystoneProperties.Pages.Shared.Components.StatusBadge;

public class StatusBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string status, string? type = null)
    {
        var badgeClass = GetBadgeClass(status, type);
        ViewBag.Status = status;
        ViewBag.BadgeClass = badgeClass;
        return View("Default");
    }

    private static string GetBadgeClass(string status, string? type)
    {
        return status.ToLower() switch
        {
            // Lease statuses
            "active" => "bg-success",
            "pending" => "bg-warning text-dark",
            "expired" => "bg-secondary",
            "renewed" => "bg-info",
            "terminated" => "bg-danger",
            // Unit statuses
            "available" => "bg-success",
            "occupied" => "bg-primary",
            "maintenance" => "bg-warning text-dark",
            "offmarket" => "bg-secondary",
            // Payment statuses
            "completed" => "bg-success",
            "failed" => "bg-danger",
            "refunded" => "bg-info",
            // Maintenance statuses
            "submitted" => "bg-info",
            "assigned" => "bg-primary",
            "inprogress" => "bg-warning text-dark",
            "cancelled" => "bg-secondary",
            // Priorities
            "low" => "bg-info",
            "medium" => "bg-warning text-dark",
            "high" => "bg-danger",
            "emergency" => "bg-danger",
            // Conditions
            "excellent" => "bg-success",
            "good" => "bg-primary",
            "fair" => "bg-warning text-dark",
            "poor" => "bg-danger",
            // Deposit statuses
            "held" => "bg-info",
            "partiallyreturned" => "bg-warning text-dark",
            "returned" => "bg-success",
            "forfeited" => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
