using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync();
    Task<MembershipPlanDto?> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto?> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task<bool> DeactivateAsync(int id);
}

public interface IMemberService
{
    Task<PaginatedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize);
    Task<MemberDetailDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto?> UpdateAsync(int id, UpdateMemberDto dto);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<PaginatedResult<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize);
    Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<IReadOnlyList<MembershipDto>> GetMembershipsAsync(int memberId);
}

public interface IMembershipService
{
    Task<MembershipDto?> GetByIdAsync(int id);
    Task<(MembershipDto? Result, string? Error)> CreateAsync(CreateMembershipDto dto);
    Task<(bool Success, string? Error)> CancelAsync(int id);
    Task<(bool Success, string? Error)> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<(bool Success, string? Error)> UnfreezeAsync(int id);
    Task<(MembershipDto? Result, string? Error)> RenewAsync(int id);
}

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorDto>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorDto?> GetByIdAsync(int id);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto);
    Task<InstructorDto?> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to);
}

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium);
    Task<ClassTypeDto?> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto?> UpdateAsync(int id, UpdateClassTypeDto dto);
}

public interface IClassScheduleService
{
    Task<PaginatedResult<ClassScheduleDto>> GetAllAsync(DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? available, int page, int pageSize);
    Task<ClassScheduleDto?> GetByIdAsync(int id);
    Task<(ClassScheduleDto? Result, string? Error)> CreateAsync(CreateClassScheduleDto dto);
    Task<(ClassScheduleDto? Result, string? Error)> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<(bool Success, string? Error)> CancelAsync(int id, CancelClassDto dto);
    Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id);
    Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id);
    Task<IReadOnlyList<ClassScheduleDto>> GetAvailableAsync();
}

public interface IBookingService
{
    Task<BookingDto?> GetByIdAsync(int id);
    Task<(BookingDto? Result, string? Error)> CreateAsync(CreateBookingDto dto);
    Task<(bool Success, string? Error)> CancelAsync(int id, CancelBookingDto dto);
    Task<(bool Success, string? Error)> CheckInAsync(int id);
    Task<(bool Success, string? Error)> MarkNoShowAsync(int id);
}
