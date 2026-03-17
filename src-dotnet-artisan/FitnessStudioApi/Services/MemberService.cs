using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResult<MemberDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize)
    {
        var query = db.Members.AsQueryable();

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

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => ToDto(m))
            .ToListAsync();

        return new PaginatedResult<MemberDto>(items, total, page, pageSize);
    }

    public async Task<MemberDetailDto?> GetByIdAsync(int id)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null) return null;

        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync();

        var bookings = await db.Bookings.Where(b => b.MemberId == id).ToListAsync();
        var now = DateTime.UtcNow;

        var stats = new BookingStatsDto(
            TotalBookings: bookings.Count,
            UpcomingBookings: bookings.Count(b => b.Status == BookingStatus.Confirmed),
            AttendedBookings: bookings.Count(b => b.Status == BookingStatus.Attended),
            NoShows: bookings.Count(b => b.Status == BookingStatus.NoShow));

        MembershipSummaryDto? membershipSummary = activeMembership is null ? null :
            new MembershipSummaryDto(activeMembership.Id, activeMembership.MembershipPlan.Name,
                activeMembership.StartDate, activeMembership.EndDate, activeMembership.Status);

        return new MemberDetailDto(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, member.CreatedAt, member.UpdatedAt,
            membershipSummary, stats);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new ValidationException("Member must be at least 16 years old.");

        if (await db.Members.AnyAsync(m => m.Email == dto.Email))
            throw new ValidationException("A member with this email already exists.");

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
        db.Members.Add(member);
        await db.SaveChangesAsync();
        logger.LogInformation("Registered member {Email} (Id={MemberId})", member.Email, member.Id);
        return ToDto(member);
    }

    public async Task<MemberDto?> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null) return null;

        if (await db.Members.AnyAsync(m => m.Email == dto.Email && m.Id != id))
            throw new ValidationException("A member with this email already exists.");

        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.DateOfBirth = dto.DateOfBirth;
        member.EmergencyContactName = dto.EmergencyContactName;
        member.EmergencyContactPhone = dto.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated member {MemberId}", id);
        return ToDto(member);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null) return (false, "Member not found.");

        var hasFutureBookings = await db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == id && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow);

        if (hasFutureBookings)
            return (false, "Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Deactivated member {MemberId}", id);
        return (true, null);
    }

    public async Task<PaginatedResult<BookingDto>> GetBookingsAsync(int memberId, string? status, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var s))
            query = query.Where(b => b.Status == s);
        if (from.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => BookingService.ToDto(b))
            .ToListAsync();

        return new PaginatedResult<BookingDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<BookingDto>> GetUpcomingBookingsAsync(int memberId)
    {
        return await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => BookingService.ToDto(b))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MembershipDto>> GetMembershipsAsync(int memberId)
    {
        return await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MembershipService.ToDto(ms))
            .ToListAsync();
    }

    private static MemberDto ToDto(Member m) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
        m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
        m.JoinDate, m.IsActive, m.CreatedAt, m.UpdatedAt);
}

public class ValidationException(string message) : Exception(message);
