using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(AppDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
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
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapToResponse(m))
            .ToListAsync(ct);

        return new PaginatedResponse<MemberResponse>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id &&
                (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

        var totalBookings = await db.Bookings.CountAsync(b => b.MemberId == id, ct);
        var upcomingBookings = await db.Bookings
            .Include(b => b.ClassSchedule)
            .CountAsync(b => b.MemberId == id &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        MembershipSummary? membershipSummary = activeMembership is not null
            ? new MembershipSummary(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.StartDate,
                activeMembership.EndDate,
                activeMembership.Status.ToString())
            : null;

        return new MemberDetailResponse(
            member.Id, member.FirstName, member.LastName, member.Email,
            member.Phone, member.DateOfBirth, member.EmergencyContactName,
            member.EmergencyContactPhone, member.JoinDate, member.IsActive,
            membershipSummary, totalBookings, upcomingBookings);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
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
        logger.LogInformation("Registered new member {MemberName} with Id {MemberId}",
            $"{member.FirstName} {member.LastName}", member.Id);
        return MapToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(member);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return (false, "Member not found");
        }

        var hasFutureBookings = await db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == id &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
        {
            return (false, "Cannot deactivate member with future bookings. Cancel bookings first.");
        }

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deactivated member {MemberId}", id);
        return (true, null);
    }

    public async Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(
        int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
        {
            query = query.Where(b => b.Status == bookingStatus);
        }

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(b => b.ClassSchedule.StartTime >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(b => b.ClassSchedule.StartTime <= to);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);

        return new PaginatedResponse<BookingResponse>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        return await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId &&
                b.Status == BookingStatus.Confirmed &&
                b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct)
    {
        return await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MapMembershipToResponse(ms))
            .ToListAsync(ct);
    }

    private static MemberResponse MapToResponse(Member m) =>
        new(m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
            m.DateOfBirth, m.EmergencyContactName, m.EmergencyContactPhone,
            m.JoinDate, m.IsActive);

    private static BookingResponse MapBookingToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
            b.ClassSchedule.StartTime, b.MemberId,
            $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
            b.CheckInTime, b.CancellationDate, b.CancellationReason);

    private static MembershipResponse MapMembershipToResponse(Membership ms) =>
        new(ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
            ms.MembershipPlanId, ms.MembershipPlan.Name, ms.StartDate, ms.EndDate,
            ms.Status.ToString(), ms.PaymentStatus.ToString(),
            ms.FreezeStartDate, ms.FreezeEndDate);
}
