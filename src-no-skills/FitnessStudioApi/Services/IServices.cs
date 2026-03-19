using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanDto>> GetAllAsync();
    Task<MembershipPlanDto> GetByIdAsync(int id);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto);
    Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto);
    Task DeleteAsync(int id);
}

public interface IMemberService
{
    Task<PagedResult<MemberListDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination);
    Task<MemberDto> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
    Task DeleteAsync(int id);
    Task<PagedResult<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination);
    Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId);
    Task<List<MembershipDto>> GetMembershipsAsync(int memberId);
}

public interface IMembershipService
{
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto> GetByIdAsync(int id);
    Task<MembershipDto> CancelAsync(int id);
    Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto);
    Task<MembershipDto> UnfreezeAsync(int id);
    Task<MembershipDto> RenewAsync(int id);
}

public interface IInstructorService
{
    Task<List<InstructorDto>> GetAllAsync(string? specialization, bool? isActive);
    Task<InstructorDto> GetByIdAsync(int id);
    Task<InstructorDto> CreateAsync(CreateInstructorDto dto);
    Task<InstructorDto> UpdateAsync(int id, UpdateInstructorDto dto);
    Task<List<ClassScheduleDto>> GetScheduleAsync(int instructorId, DateTime? fromDate, DateTime? toDate);
}

public interface IClassTypeService
{
    Task<List<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium);
    Task<ClassTypeDto> GetByIdAsync(int id);
    Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto);
    Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto);
}

public interface IClassScheduleService
{
    Task<PagedResult<ClassScheduleDto>> GetAllAsync(DateTime? fromDate, DateTime? toDate, int? classTypeId, int? instructorId, bool? hasAvailability, PaginationParams pagination);
    Task<ClassScheduleDto> GetByIdAsync(int id);
    Task<ClassScheduleDto> CreateAsync(CreateClassScheduleDto dto);
    Task<ClassScheduleDto> UpdateAsync(int id, UpdateClassScheduleDto dto);
    Task<ClassScheduleDto> CancelAsync(int id, CancelClassDto dto);
    Task<List<BookingDto>> GetRosterAsync(int id);
    Task<List<BookingDto>> GetWaitlistAsync(int id);
    Task<List<ClassScheduleDto>> GetAvailableAsync();
}

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto> GetByIdAsync(int id);
    Task<BookingDto> CancelAsync(int id, CancelBookingDto dto);
    Task<BookingDto> CheckInAsync(int id);
    Task<BookingDto> NoShowAsync(int id);
}
