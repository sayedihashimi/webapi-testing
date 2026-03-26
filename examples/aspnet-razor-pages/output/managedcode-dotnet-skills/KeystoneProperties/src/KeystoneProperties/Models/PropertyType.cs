using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum PropertyType
{
    Apartment,
    Townhouse,

    [Display(Name = "Single Family")]
    SingleFamily,

    Condo
}
