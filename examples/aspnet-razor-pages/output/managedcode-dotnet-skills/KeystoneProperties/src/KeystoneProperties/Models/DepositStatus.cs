using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum DepositStatus
{
    Held,

    [Display(Name = "Partially Returned")]
    PartiallyReturned,

    Returned,
    Forfeited
}
