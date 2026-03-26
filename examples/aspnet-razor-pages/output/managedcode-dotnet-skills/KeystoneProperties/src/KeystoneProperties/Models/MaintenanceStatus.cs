using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum MaintenanceStatus
{
    Submitted,
    Assigned,

    [Display(Name = "In Progress")]
    InProgress,

    Completed,
    Cancelled
}
