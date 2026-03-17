using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class MemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;

    public MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<MemberDto>> GetAllAsync(string? search, bool? isActive, int page = 1, int pageSize = 10)
    {
        var query = _db.Members.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(s) ||
                m.LastName.ToLower().Contains(s) ||
                m.Email.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapToDto(m))
            .ToListAsync();

        return new PaginatedResponse<MemberDto>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MemberDetailDto?> GetByIdAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return null;

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen);

        return new MemberDetailDto
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
            CreatedAt = member.CreatedAt,
            UpdatedAt = member.UpdatedAt,
            ActiveMembership = activeMembership == null ? null : new MembershipSummaryDto
            {
                Id = activeMembership.Id,
                PlanName = activeMembership.MembershipPlan.Name,
                Status = activeMembership.Status,
                StartDate = activeMembership.StartDate,
                EndDate = activeMembership.EndDate
            },
            TotalBookings = member.Bookings.Count,
            AttendedClasses = member.Bookings.Count(b => b.Status == BookingStatus.Attended),
            NoShows = member.Bookings.Count(b => b.Status == BookingStatus.NoShow)
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
        _logger.LogInformation("Created member {MemberEmail} with ID {MemberId}", member.Email, member.Id);
        return MapToDto(member);
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new BusinessRuleException($"Member with ID {id} not found.", 404);

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.", 409);

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.IsActive = dto.IsActive;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated member {MemberId}", id);
        return MapToDto(member);
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException($"Member with ID {id} not found.", 404);

        var hasFutureBookings = member.Bookings.Any(b =>
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
            _db.ClassSchedules.Any(cs => cs.Id == b.ClassScheduleId && cs.StartTime > DateTime.UtcNow));

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot delete member with future bookings. Cancel bookings first.", 409);

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated member {MemberId}", id);
    }

    public async Task<PaginatedResponse<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException($"Member with ID {memberId} not found.", 404);

        var query = _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (from.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => BookingService.MapToDto(b))
            .ToListAsync();

        return new PaginatedResponse<BookingDto>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException($"Member with ID {memberId} not found.", 404);

        return await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                        b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => BookingService.MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<MembershipDto>> GetMembershipsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException($"Member with ID {memberId} not found.", 404);

        return await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MembershipService.MapToDto(ms))
            .ToListAsync();
    }

    private static MemberDto MapToDto(Member m) => new()
    {
        Id = m.Id,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        Phone = m.Phone,
        DateOfBirth = m.DateOfBirth,
        EmergencyContactName = m.EmergencyContactName,
        EmergencyContactPhone = m.EmergencyContactPhone,
        JoinDate = m.JoinDate,
        IsActive = m.IsActive,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
