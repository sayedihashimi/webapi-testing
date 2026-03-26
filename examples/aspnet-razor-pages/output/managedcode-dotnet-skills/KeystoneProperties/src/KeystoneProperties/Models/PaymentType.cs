using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum PaymentType
{
    Rent,

    [Display(Name = "Late Fee")]
    LateFee,

    Deposit,

    [Display(Name = "Deposit Return")]
    DepositReturn,

    Other
}
