using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum InspectionType
{
    [Display(Name = "Move In")]
    MoveIn,

    [Display(Name = "Move Out")]
    MoveOut,

    Routine,
    Emergency
}
