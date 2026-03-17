using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

// ── MembershipPlan ──
public record MembershipPlanDto(
    int Id, string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses,
    bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public record UpdateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive);

// ── Member ──
public record MemberDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record MemberDetailDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt,
    MembershipSummaryDto? ActiveMembership, BookingStatsDto BookingStats);

public record MembershipSummaryDto(int Id, string PlanName, DateOnly StartDate, DateOnly EndDate, MembershipStatus Status);
public record BookingStatsDto(int TotalBookings, int UpcomingBookings, int AttendedBookings, int NoShows);

public record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

public record UpdateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

// ── Membership ──
public record MembershipDto(
    int Id, int MemberId, string MemberName, int MembershipPlanId, string PlanName,
    DateOnly StartDate, DateOnly EndDate, MembershipStatus Status,
    PaymentStatus PaymentStatus, DateOnly? FreezeStartDate, DateOnly? FreezeEndDate,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateMembershipDto(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    PaymentStatus PaymentStatus = PaymentStatus.Paid);

public record FreezeMembershipDto(
    [Range(7, 30)] int FreezeDays);

// ── Instructor ──
public record InstructorDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Bio, string? Specializations, DateOnly HireDate,
    bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateInstructorDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations,
    DateOnly HireDate);

public record UpdateInstructorDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations,
    bool IsActive);

// ── ClassType ──
public record ClassTypeDto(
    int Id, string Name, string? Description, int DefaultDurationMinutes,
    int DefaultCapacity, bool IsPremium, int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateClassTypeDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel);

public record UpdateClassTypeDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel,
    bool IsActive);

// ── ClassSchedule ──
public record ClassScheduleDto(
    int Id, int ClassTypeId, string ClassTypeName, int InstructorId,
    string InstructorName, DateTime StartTime, DateTime EndTime,
    int Capacity, int CurrentEnrollment, int WaitlistCount, string Room,
    ClassScheduleStatus Status, string? CancellationReason,
    int AvailableSpots, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateClassScheduleDto(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    [Range(1, 100)] int Capacity,
    [Required, MaxLength(50)] string Room);

public record UpdateClassScheduleDto(
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    [Range(1, 100)] int Capacity,
    [Required, MaxLength(50)] string Room);

public record CancelClassDto(string? Reason);

// ── Booking ──
public record BookingDto(
    int Id, int ClassScheduleId, string ClassName, int MemberId, string MemberName,
    DateTime BookingDate, BookingStatus Status, int? WaitlistPosition,
    DateTime? CheckInTime, DateTime? CancellationDate, string? CancellationReason,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateBookingDto(int ClassScheduleId, int MemberId);

public record CancelBookingDto(string? Reason);

// ── Pagination ──
public record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// ── Roster / Waitlist ──
public record RosterEntryDto(int BookingId, int MemberId, string MemberName, DateTime BookingDate);
public record WaitlistEntryDto(int BookingId, int MemberId, string MemberName, int? WaitlistPosition, DateTime BookingDate);
