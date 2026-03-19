using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;

    public MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<MemberListDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination)
    {
        var query = _db.Members.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(s) ||
                m.LastName.ToLower().Contains(s) ||
                m.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(m => new MemberListDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                Phone = m.Phone,
                IsActive = m.IsActive,
                JoinDate = m.JoinDate
            })
            .ToListAsync();

        return new PagedResult<MemberListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<MemberDto> GetByIdAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen);

        var now = DateTime.UtcNow;
        return new MemberDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone,
            DateOfBirth = member.DateOfBirth,
            EmergencyContactName = member.EmergencyContactName,
            EmergencyContactPhone = member.EmergencyContactPhone,
            JoinDate = member.JoinDate,
            IsActive = member.IsActive,
            ActiveMembership = activeMembership == null ? null : new MembershipSummaryDto
            {
                Id = activeMembership.Id,
                PlanName = activeMembership.MembershipPlan.Name,
                Status = activeMembership.Status.ToString(),
                StartDate = activeMembership.StartDate,
                EndDate = activeMembership.EndDate
            },
            TotalBookings = member.Bookings.Count,
            UpcomingBookings = member.Bookings.Count(b =>
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule != null &&
                b.ClassSchedule.StartTime > now)
        };
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var age = DateOnly.FromDateTime(DateTime.Today).DayNumber - dto.DateOfBirth.DayNumber;
        var years = (DateTime.Today - dto.DateOfBirth.ToDateTime(TimeOnly.MinValue)).Days / 365.25;
        if (years < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409);

        var member = new Member
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Registered new member: {Email}", member.Email);
        return await GetByIdAsync(member.Id);
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409);

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(member.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Bookings).ThenInclude(b => b.ClassSchedule)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = member.Bookings.Any(b =>
            b.Status == BookingStatus.Confirmed &&
            b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated member: {Email}", member.Email);
    }

    public async Task<PagedResult<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (fromDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<BookingDto>
        {
            Items = items.Select(MapBookingToDto).ToList(),
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var bookings = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                        b.Status == BookingStatus.Confirmed &&
                        b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync();

        return bookings.Select(MapBookingToDto).ToList();
    }

    public async Task<List<MembershipDto>> GetMembershipsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var memberships = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync();

        return memberships.Select(MembershipService.MapToDto).ToList();
    }

    internal static BookingDto MapBookingToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule.ClassType.Name,
        ClassStartTime = b.ClassSchedule.StartTime,
        ClassEndTime = b.ClassSchedule.EndTime,
        Room = b.ClassSchedule.Room,
        InstructorName = $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        MemberId = b.MemberId,
        MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
        BookingDate = b.BookingDate,
        Status = b.Status.ToString(),
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason
    };
}
