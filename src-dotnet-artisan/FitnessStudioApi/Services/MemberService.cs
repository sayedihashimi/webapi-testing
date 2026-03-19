using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PagedResult<MemberResponse>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Members.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var members = await query
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MemberResponse>(
            members.Select(MapToResponse).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members
            .Include(m => m.Memberships)
                .ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null)
        {
            return null;
        }

        var activeMembership = member.Memberships
            .FirstOrDefault(ms => ms.Status is MembershipStatus.Active or MembershipStatus.Frozen);

        MembershipResponse? activeMembershipResponse = null;
        if (activeMembership is not null)
        {
            activeMembershipResponse = MapMembershipToResponse(activeMembership);
        }

        return new MemberDetailResponse(
            member.Id,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Phone,
            member.DateOfBirth,
            member.EmergencyContactName,
            member.EmergencyContactPhone,
            member.JoinDate,
            member.IsActive,
            activeMembershipResponse,
            member.Bookings.Count,
            member.Bookings.Count(b => b.Status == BookingStatus.Attended),
            member.CreatedAt,
            member.UpdatedAt);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        if (age < 16)
        {
            throw new InvalidOperationException("Member must be at least 16 years old.");
        }

        if (await db.Members.AnyAsync(m => m.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created member {MemberName} with ID {MemberId}", $"{member.FirstName} {member.LastName}", member.Id);

        return MapToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        if (age < 16)
        {
            throw new InvalidOperationException("Member must be at least 16 years old.");
        }

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.DateOfBirth = request.DateOfBirth;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated member {MemberId}", id);

        return MapToResponse(member);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null)
        {
            return (false, "Member not found.");
        }

        var hasFutureBookings = member.Bookings.Any(b =>
            b.Status is BookingStatus.Confirmed or BookingStatus.Waitlisted);

        if (hasFutureBookings)
        {
            return (false, "Cannot delete member with active or waitlisted bookings. Cancel bookings first.");
        }

        db.Members.Remove(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted member {MemberId}", id);

        return (true, null);
    }

    public async Task<PagedResult<BookingResponse>> GetMemberBookingsAsync(
        int memberId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
        {
            query = query.Where(b => b.Status == bookingStatus);
        }

        var totalCount = await query.CountAsync(ct);

        var bookings = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<BookingResponse>(
            bookings.Select(MapBookingToResponse).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var bookings = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                        b.Status == BookingStatus.Confirmed &&
                        b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync(ct);

        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct = default)
    {
        var memberships = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync(ct);

        return memberships.Select(MapMembershipToResponse).ToList();
    }

    private static MemberResponse MapToResponse(Member member) => new(
        member.Id,
        member.FirstName,
        member.LastName,
        member.Email,
        member.Phone,
        member.DateOfBirth,
        member.EmergencyContactName,
        member.EmergencyContactPhone,
        member.JoinDate,
        member.IsActive,
        member.CreatedAt,
        member.UpdatedAt);

    private static MembershipResponse MapMembershipToResponse(Membership ms) => new(
        ms.Id,
        ms.MemberId,
        $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId,
        ms.MembershipPlan.Name,
        ms.StartDate,
        ms.EndDate,
        ms.Status,
        ms.PaymentStatus,
        ms.FreezeStartDate,
        ms.FreezeEndDate,
        ms.CreatedAt,
        ms.UpdatedAt);

    private static BookingResponse MapBookingToResponse(Booking b) => new(
        b.Id,
        b.ClassScheduleId,
        b.ClassSchedule.ClassType.Name,
        b.MemberId,
        $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate,
        b.Status,
        b.WaitlistPosition,
        b.CheckInTime,
        b.CancellationDate,
        b.CancellationReason,
        b.ClassSchedule.StartTime,
        b.ClassSchedule.EndTime,
        b.ClassSchedule.Room,
        $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        b.CreatedAt,
        b.UpdatedAt);
}
