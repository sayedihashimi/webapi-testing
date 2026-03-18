using FitnessStudioApi.Models.Enums;

namespace FitnessStudioApi.DTOs;

// ========== MembershipPlan DTOs ==========
public record MembershipPlanDto(
    int Id, string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses,
    bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateMembershipPlanDto(
    string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses);

public record UpdateMembershipPlanDto(
    string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses, bool IsActive);

// ========== Member DTOs ==========
public record MemberDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record MemberDetailDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt,
    MembershipDto? ActiveMembership, int TotalBookings, int UpcomingBookings);

public record CreateMemberDto(
    string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone);

public record UpdateMemberDto(
    string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone);

// ========== Membership DTOs ==========
public record MembershipDto(
    int Id, int MemberId, string MemberName, int MembershipPlanId, string PlanName,
    DateOnly StartDate, DateOnly EndDate, MembershipStatus Status,
    PaymentStatus PaymentStatus, DateOnly? FreezeStartDate, DateOnly? FreezeEndDate,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateMembershipDto(int MemberId, int MembershipPlanId, DateOnly StartDate, PaymentStatus PaymentStatus = PaymentStatus.Paid);

public record FreezeMembershipDto(int FreezeDurationDays);

// ========== Instructor DTOs ==========
public record InstructorDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Bio, string? Specializations, DateOnly HireDate, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateInstructorDto(
    string FirstName, string LastName, string Email, string Phone,
    string? Bio, string? Specializations, DateOnly HireDate);

public record UpdateInstructorDto(
    string FirstName, string LastName, string Email, string Phone,
    string? Bio, string? Specializations, bool IsActive);

// ========== ClassType DTOs ==========
public record ClassTypeDto(
    int Id, string Name, string? Description, int DefaultDurationMinutes,
    int DefaultCapacity, bool IsPremium, int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateClassTypeDto(
    string Name, string? Description, int DefaultDurationMinutes,
    int DefaultCapacity, bool IsPremium, int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel);

public record UpdateClassTypeDto(
    string Name, string? Description, int DefaultDurationMinutes,
    int DefaultCapacity, bool IsPremium, int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel, bool IsActive);

// ========== ClassSchedule DTOs ==========
public record ClassScheduleDto(
    int Id, int ClassTypeId, string ClassTypeName, int InstructorId,
    string InstructorName, DateTime StartTime, DateTime EndTime,
    int Capacity, int CurrentEnrollment, int WaitlistCount, string Room,
    ClassScheduleStatus Status, string? CancellationReason,
    DateTime CreatedAt, DateTime UpdatedAt);

public record ClassScheduleDetailDto(
    int Id, int ClassTypeId, string ClassTypeName, int InstructorId,
    string InstructorName, DateTime StartTime, DateTime EndTime,
    int Capacity, int CurrentEnrollment, int WaitlistCount,
    int AvailableSpots, string Room, ClassScheduleStatus Status,
    string? CancellationReason, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateClassScheduleDto(
    int ClassTypeId, int InstructorId, DateTime StartTime,
    int? DurationMinutes, int? Capacity, string Room);

public record UpdateClassScheduleDto(
    int ClassTypeId, int InstructorId, DateTime StartTime,
    DateTime EndTime, int Capacity, string Room);

public record CancelClassDto(string? Reason);

// ========== Booking DTOs ==========
public record BookingDto(
    int Id, int ClassScheduleId, string ClassName, int MemberId,
    string MemberName, DateTime BookingDate, BookingStatus Status,
    int? WaitlistPosition, DateTime? CheckInTime,
    DateTime? CancellationDate, string? CancellationReason,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateBookingDto(int ClassScheduleId, int MemberId);

public record CancelBookingDto(string? Reason);

// ========== Roster/Waitlist DTOs ==========
public record RosterEntryDto(int MemberId, string MemberName, string Email, BookingStatus Status, DateTime BookingDate, DateTime? CheckInTime);

public record WaitlistEntryDto(int MemberId, string MemberName, int? WaitlistPosition, DateTime BookingDate);

// ========== Pagination ==========
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
