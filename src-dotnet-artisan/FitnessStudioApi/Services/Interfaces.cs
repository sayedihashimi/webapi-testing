using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct);
    Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct);
    Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
}

public interface IMemberService
{
    Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct);
    Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);
    Task<IReadOnlyList<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct);
}

public interface IMembershipService
{
    Task<(MembershipResponse? Result, string? Error)> CreateAsync(CreateMembershipRequest request, CancellationToken ct);
    Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken ct);
    Task<(bool Success, string? Error)> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct);
    Task<(bool Success, string? Error)> UnfreezeAsync(int id, CancellationToken ct);
    Task<(MembershipResponse? Result, string? Error)> RenewAsync(int id, CancellationToken ct);
}

public interface IInstructorService
{
    Task<IReadOnlyList<InstructorResponse>> GetAllAsync(string? specialization, bool? isActive, CancellationToken ct);
    Task<InstructorResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct);
    Task<InstructorResponse?> UpdateAsync(int id, UpdateInstructorRequest request, CancellationToken ct);
    Task<IReadOnlyList<ClassScheduleResponse>> GetScheduleAsync(int instructorId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct);
}

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, CancellationToken ct);
    Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct);
    Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct);
}

public interface IClassScheduleService
{
    Task<PaginatedResponse<ClassScheduleResponse>> GetAllAsync(DateOnly? fromDate, DateOnly? toDate, int? classTypeId, int? instructorId, bool? available, int page, int pageSize, CancellationToken ct);
    Task<ClassScheduleResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<(ClassScheduleResponse? Result, string? Error)> CreateAsync(CreateClassScheduleRequest request, CancellationToken ct);
    Task<(ClassScheduleResponse? Result, string? Error)> UpdateAsync(int id, UpdateClassScheduleRequest request, CancellationToken ct);
    Task<(bool Success, string? Error)> CancelAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ClassRosterEntry>> GetRosterAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<WaitlistEntry>> GetWaitlistAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ClassScheduleResponse>> GetAvailableAsync(CancellationToken ct);
}

public interface IBookingService
{
    Task<(BookingResponse? Result, string? Error)> CreateAsync(CreateBookingRequest request, CancellationToken ct);
    Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<(bool Success, string? Error)> CancelAsync(int id, CancelBookingRequest? request, CancellationToken ct);
    Task<(bool Success, string? Error)> CheckInAsync(int id, CancellationToken ct);
    Task<(bool Success, string? Error)> MarkNoShowAsync(int id, CancellationToken ct);
}
