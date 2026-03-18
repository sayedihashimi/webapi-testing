using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// --- MembershipPlan DTOs ---

public sealed record CreateMembershipPlanRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public sealed record UpdateMembershipPlanRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public sealed record MembershipPlanResponse(
    int Id,
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive);

// --- Member DTOs ---

public sealed record CreateMemberRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

public sealed record UpdateMemberRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

public sealed record MemberResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    MembershipSummary? ActiveMembership);

public sealed record MembershipSummary(
    int Id,
    string PlanName,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate);

// --- Membership DTOs ---

public sealed record CreateMembershipRequest(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    string PaymentStatus = "Paid");

public sealed record MembershipResponse(
    int Id,
    int MemberId,
    string MemberName,
    int MembershipPlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate);

public sealed record FreezeMembershipRequest(
    [Range(7, 30)] int DurationDays);

// --- Instructor DTOs ---

public sealed record CreateInstructorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations,
    DateOnly HireDate);

public sealed record UpdateInstructorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations);

public sealed record InstructorResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive);

// --- ClassType DTOs ---

public sealed record CreateClassTypeRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel);

public sealed record UpdateClassTypeRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel);

public sealed record ClassTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel,
    bool IsActive);

// --- ClassSchedule DTOs ---

public sealed record CreateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    [Range(1, 100)] int Capacity,
    [Required, MaxLength(50)] string Room);

public sealed record UpdateClassScheduleRequest(
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    [Range(1, 100)] int Capacity,
    [Required, MaxLength(50)] string Room);

public sealed record ClassScheduleResponse(
    int Id,
    int ClassTypeId,
    string ClassTypeName,
    int InstructorId,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int WaitlistCount,
    int AvailableSpots,
    string Room,
    string Status,
    string? CancellationReason);

public sealed record CancelClassRequest(string Reason);

// --- Booking DTOs ---

public sealed record CreateBookingRequest(
    int ClassScheduleId,
    int MemberId);

public sealed record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassName,
    DateTime ClassStartTime,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    string Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason);

public sealed record CancelBookingRequest(string? Reason);

// --- Roster/Waitlist ---

public sealed record RosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    string Status);

// --- Pagination ---

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
