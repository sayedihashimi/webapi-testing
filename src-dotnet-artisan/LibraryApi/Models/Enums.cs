namespace LibraryApi.Models;

public enum MembershipType
{
    Standard,
    Premium,
    Student
}

public enum LoanStatus
{
    Active,
    Returned,
    Overdue
}

public enum ReservationStatus
{
    Pending,
    Ready,
    Fulfilled,
    Cancelled,
    Expired
}

public enum FineStatus
{
    Unpaid,
    Paid,
    Waived
}
