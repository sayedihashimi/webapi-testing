using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services.Interfaces;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync();
    Task<MembershipPlanDto?> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task DeactivateAsync(int id);
}

public interface IMemberService
{
    Task<PagedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize);
    Task<MemberDetailDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
    Task DeactivateAsync(int id);
    Task<PagedResult<BookingDto>> GetMemberBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize);
    Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<IReadOnlyList<MembershipDto>> GetMemberMembershipsAsync(int memberId);
}

public interface IMembershipService
{
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto?> GetByIdAsync(int id);
    Task<MembershipDto> CancelAsync(int id);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipDto> UnfreezeAsync(int id);
    Task<MembershipDto> RenewAsync(int id);
}

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorDto>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorDto?> GetByIdAsync(int id);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto);
    Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<IReadOnlyList<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? from, DateTime? to);
}

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium);
    Task<ClassTypeDto?> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto);
}

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleDto>> GetAllAsync(DateTime? from, DateTime? to, int? classTypeId, int? instructorId, bool? hasAvailability, int page, int pageSize);
    Task<ClassScheduleDetailDto?> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task CancelAsync(int id, CancelClassDto dto);
    Task<IReadOnlyList<RosterEntryDto>> GetRosterAsync(int id);
    Task<IReadOnlyList<WaitlistEntryDto>> GetWaitlistAsync(int id);
    Task<IReadOnlyList<ClassScheduleDto>> GetAvailableAsync();
}

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int id);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto);
    Task<BookingDto> CheckInAsync(int id);
    Task<BookingDto> MarkNoShowAsync(int id);
}
