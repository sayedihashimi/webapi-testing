namespace HorizonHR.Models;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Intern
}

public enum EmployeeStatus
{
    Active,
    OnLeave,
    Terminated
}

public enum LeaveRequestStatus
{
    Submitted,
    Approved,
    Rejected,
    Cancelled
}

public enum ReviewStatus
{
    Draft,
    SelfAssessmentPending,
    ManagerReviewPending,
    Completed
}

public enum OverallRating
{
    Outstanding,
    ExceedsExpectations,
    MeetsExpectations,
    NeedsImprovement,
    Unsatisfactory
}

public enum ProficiencyLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}
