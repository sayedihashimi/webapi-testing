using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

// === Pagination ===
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

// === MembershipPlan DTOs ===
public sealed record CreateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public sealed record UpdateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
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

// === Member DTOs ===
public sealed record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone);

public sealed record UpdateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string EmergencyContactName,
    string EmergencyContactPhone);

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
    bool IsActive);

public sealed record MemberDetailResponse(
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
    MembershipSummary? ActiveMembership,
    int TotalBookings,
    int UpcomingBookings);

public sealed record MembershipSummary(
    int Id,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

// === Membership DTOs ===
public sealed record CreateMembershipRequest(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    string PaymentStatus);

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

public sealed record FreezeMembershipRequest(int FreezeDurationDays);

// === Instructor DTOs ===
public sealed record CreateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate);

public sealed record UpdateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
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

// === ClassType DTOs ===
public sealed record CreateClassTypeRequest(
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel);

public sealed record UpdateClassTypeRequest(
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel);

public sealed record ClassTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel,
    bool IsActive);

// === ClassSchedule DTOs ===
public sealed record CreateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    int? DurationMinutes,
    int? Capacity,
    string Room);

public sealed record UpdateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    int? DurationMinutes,
    int? Capacity,
    string Room);

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

public sealed record ClassRosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    string Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record WaitlistEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);

// === Booking DTOs ===
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
