using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public enum PaymentMethod
{
    Check,

    [Display(Name = "Bank Transfer")]
    BankTransfer,

    [Display(Name = "Credit Card")]
    CreditCard,

    Cash,

    [Display(Name = "Money Order")]
    MoneyOrder
}
