using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// === MembershipPlan DTOs ===
public class CreateMembershipPlanDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 24)]
    public int DurationMonths { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal Price { get; set; }

    public int MaxClassBookingsPerWeek { get; set; }
    public bool AllowsPremiumClasses { get; set; }
}

public class UpdateMembershipPlanDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 24)]
    public int DurationMonths { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal Price { get; set; }

    public int MaxClassBookingsPerWeek { get; set; }
    public bool AllowsPremiumClasses { get; set; }
    public bool IsActive { get; set; }
}

public class MembershipPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public int MaxClassBookingsPerWeek { get; set; }
    public bool AllowsPremiumClasses { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// === Member DTOs ===
public class CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

public class UpdateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

public class MemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateOnly JoinDate { get; set; }
    public bool IsActive { get; set; }
    public MembershipSummaryDto? ActiveMembership { get; set; }
    public int TotalBookings { get; set; }
    public int UpcomingBookings { get; set; }
}

public class MemberListDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateOnly JoinDate { get; set; }
}

public class MembershipSummaryDto
{
    public int Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

// === Membership DTOs ===
public class CreateMembershipDto
{
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
}

public class MembershipDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int MembershipPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FreezeMembershipDto
{
    [Range(7, 30, ErrorMessage = "Freeze duration must be between 7 and 30 days")]
    public int FreezeDurationDays { get; set; }
}

// === Instructor DTOs ===
public class CreateInstructorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Specializations { get; set; }

    [Required]
    public DateOnly HireDate { get; set; }
}

public class UpdateInstructorDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Specializations { get; set; }
    public bool IsActive { get; set; }
}

public class InstructorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
}

// === ClassType DTOs ===
public class CreateClassTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(30, 120)]
    public int DefaultDurationMinutes { get; set; }

    [Range(1, 50)]
    public int DefaultCapacity { get; set; }

    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }

    [Required]
    public string DifficultyLevel { get; set; } = string.Empty;
}

public class UpdateClassTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(30, 120)]
    public int DefaultDurationMinutes { get; set; }

    [Range(1, 50)]
    public int DefaultCapacity { get; set; }

    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }

    [Required]
    public string DifficultyLevel { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class ClassTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// === ClassSchedule DTOs ===
public class CreateClassScheduleDto
{
    public int ClassTypeId { get; set; }
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 100)]
    public int Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class UpdateClassScheduleDto
{
    public int ClassTypeId { get; set; }
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 100)]
    public int Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class CancelClassDto
{
    public string? CancellationReason { get; set; }
}

public class ClassScheduleDto
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int WaitlistCount { get; set; }
    public int AvailableSpots => Capacity - CurrentEnrollment;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public bool IsPremium { get; set; }
}

// === Booking DTOs ===
public class CreateBookingDto
{
    public int ClassScheduleId { get; set; }
    public int MemberId { get; set; }
}

public class CancelBookingDto
{
    public string? CancellationReason { get; set; }
}

public class BookingDto
{
    public int Id { get; set; }
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime ClassStartTime { get; set; }
    public DateTime ClassEndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
}

// === Pagination ===
public class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : (value < 1 ? 1 : value);
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
