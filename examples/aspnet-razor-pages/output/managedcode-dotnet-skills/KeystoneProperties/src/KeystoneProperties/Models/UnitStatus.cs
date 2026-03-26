using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum UnitStatus
{
    Available,
    Occupied,
    Maintenance,

    [Display(Name = "Off Market")]
    OffMarket
}
