using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedResult<MemberResponseDto>> GetAllAsync(string? search, bool? isActive, PaginationParams pagination)
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
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .ToListAsync();

        return new PagedResult<MemberResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<MemberResponseDto?> GetByIdAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);
        return member is null ? null : MapToDto(member);
    }

    public async Task<MemberResponseDto> CreateAsync(MemberCreateDto dto)
    {
        // Age validation
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new BusinessRuleException("Member must be at least 16 years old.");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.");

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
        _logger.LogInformation("Registered new member {MemberName} with ID {MemberId}", $"{member.FirstName} {member.LastName}", member.Id);
        return MapToDto(member);
    }

    public async Task<MemberResponseDto?> UpdateAsync(int id, MemberUpdateDto dto)
    {
        var member = await _db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (member is null) return null;

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new BusinessRuleException($"A member with email '{dto.Email}' already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated member {MemberId}", id);
        return MapToDto(member);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var member = await _db.Members
            .Include(m => m.Bookings).ThenInclude(b => b.ClassSchedule)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (member is null) return false;

        var hasFutureBookings = member.Bookings.Any(b =>
            b.Status == BookingStatus.Confirmed &&
            b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated member {MemberId}", id);
        return true;
    }

    public async Task<PagedResult<BookingResponseDto>> GetBookingsAsync(int memberId, string? status, DateTime? fromDate, DateTime? toDate, PaginationParams pagination)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404);

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

        return new PagedResult<BookingResponseDto>
        {
            Items = items.Select(BookingService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<List<BookingResponseDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404);

        var now = DateTime.UtcNow;
        return await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => BookingService.MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<MembershipResponseDto>> GetMembershipsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new BusinessRuleException("Member not found.", 404);

        return await _db.Memberships
            .Include(m => m.MembershipPlan)
            .Include(m => m.Member)
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.StartDate)
            .Select(m => MembershipService.MapToDto(m))
            .ToListAsync();
    }

    private static MemberResponseDto MapToDto(Member m)
    {
        var activeMembership = m.Memberships?
            .FirstOrDefault(ms => ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen);

        return new MemberResponseDto
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
            UpdatedAt = m.UpdatedAt,
            ActiveMembership = activeMembership is null ? null : new MembershipSummaryDto
            {
                Id = activeMembership.Id,
                PlanName = activeMembership.MembershipPlan?.Name ?? "Unknown",
                Status = activeMembership.Status.ToString(),
                StartDate = activeMembership.StartDate,
                EndDate = activeMembership.EndDate
            }
        };
    }
}
