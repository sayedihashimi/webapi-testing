namespace KeystoneProperties.Models;

public enum PropertyType
{
    Apartment,
    Townhouse,
    SingleFamily,
    Condo
}

public enum UnitStatus
{
    Available,
    Occupied,
    Maintenance,
    OffMarket
}

public enum LeaseStatus
{
    Pending,
    Active,
    Expired,
    Renewed,
    Terminated
}

public enum DepositStatus
{
    Held,
    PartiallyReturned,
    Returned,
    Forfeited
}

public enum PaymentMethod
{
    Check,
    BankTransfer,
    CreditCard,
    Cash,
    MoneyOrder
}

public enum PaymentType
{
    Rent,
    LateFee,
    Deposit,
    DepositReturn,
    Other
}

public enum PaymentStatus
{
    Completed,
    Pending,
    Failed,
    Refunded
}

public enum MaintenancePriority
{
    Low,
    Medium,
    High,
    Emergency
}

public enum MaintenanceStatus
{
    Submitted,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}

public enum MaintenanceCategory
{
    Plumbing,
    Electrical,
    HVAC,
    Appliance,
    Structural,
    Pest,
    General
}

public enum InspectionType
{
    MoveIn,
    MoveOut,
    Routine,
    Emergency
}

public enum OverallCondition
{
    Excellent,
    Good,
    Fair,
    Poor
}
