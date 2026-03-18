using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;

    public MemberService(FitnessDbContext db) => _db = db;

    public async Task<PagedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize)
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

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => ToDto(m))
            .ToListAsync();

        return new PagedResult<MemberDto>(items, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<MemberDetailDto?> GetByIdAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member is null) return null;

        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync();

        var totalBookings = await _db.Bookings.CountAsync(b => b.MemberId == id);
        var upcomingBookings = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .CountAsync(b => b.MemberId == id && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow);

        MembershipDto? membershipDto = activeMembership is null ? null : MembershipService.ToDto(activeMembership);

        return new MemberDetailDto(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, member.CreatedAt, member.UpdatedAt,
            membershipDto, totalBookings, upcomingBookings);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            throw new ConflictException($"A member with email '{dto.Email}' already exists.");

        var age = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - dto.DateOfBirth.DayNumber;
        var years = (DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(TimeOnly.MinValue) - dto.DateOfBirth.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25;
        if (years < 16)
            throw new BusinessRuleException("Members must be at least 16 years old.");

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
        return ToDto(member);
    }

    public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new NotFoundException($"Member with ID {id} not found.");

        if (await _db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new ConflictException($"A member with email '{dto.Email}' already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.DateOfBirth = dto.DateOfBirth;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(member);
    }

    public async Task DeactivateAsync(int id)
    {
        var member = await _db.Members.FindAsync(id)
            ?? throw new NotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == id && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            throw new BusinessRuleException("Cannot deactivate member with future bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<BookingDto>> GetMemberBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found.");

        var query = _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (from.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(BookingService.ToDto).ToList();
        return new PagedResult<BookingDto>(dtos, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found.");

        var bookings = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync();

        return bookings.Select(BookingService.ToDto).ToList();
    }

    public async Task<IReadOnlyList<MembershipDto>> GetMemberMembershipsAsync(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            throw new NotFoundException($"Member with ID {memberId} not found.");

        var memberships = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync();

        return memberships.Select(MembershipService.ToDto).ToList();
    }

    internal static MemberDto ToDto(Member m) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
        m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
        m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt);
}
