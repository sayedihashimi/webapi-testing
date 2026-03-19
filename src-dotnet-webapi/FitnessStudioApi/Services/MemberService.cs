using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberListResponse>> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Members.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberListResponse(
                m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.JoinDate, m.IsActive))
            .ToListAsync(ct);

        return PaginatedResponse<MemberListResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members
            .AsNoTracking()
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (member is null) return null;

        var activeMembership = member.Memberships
            .FirstOrDefault(m => m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen);

        MembershipResponse? activeMembershipResponse = null;
        if (activeMembership is not null)
        {
            activeMembershipResponse = MapMembershipToResponse(activeMembership, member);
        }

        return new MemberResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, activeMembershipResponse,
            member.Bookings.Count, member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        // Age check: must be at least 16
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;
        if (age < 16)
            throw new ArgumentException("Member must be at least 16 years old.");

        var existingEmail = await db.Members.AnyAsync(m => m.Email == request.Email, ct);
        if (existingEmail)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            JoinDate = today,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created member {MemberId}: {Name}", member.Id, $"{member.FirstName} {member.LastName}");

        return new MemberResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, null, 0, member.CreatedAt, member.UpdatedAt);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members
            .Include(m => m.Memberships).ThenInclude(ms => ms.MembershipPlan)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var duplicate = await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct);
        if (duplicate)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated member {MemberId}", member.Id);

        var activeMembership = member.Memberships
            .FirstOrDefault(m => m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen);

        MembershipResponse? activeMembershipResponse = activeMembership is not null
            ? MapMembershipToResponse(activeMembership, member)
            : null;

        return new MemberResponse(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.EmergencyContactName, member.EmergencyContactPhone,
            member.JoinDate, member.IsActive, activeMembershipResponse,
            member.Bookings.Count, member.CreatedAt, member.UpdatedAt);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var member = await db.Members
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Member with ID {id} not found.");

        var hasFutureBookings = member.Bookings.Any(b =>
            b.Status == BookingStatus.Confirmed &&
            db.ClassSchedules.Any(cs => cs.Id == b.ClassScheduleId && cs.StartTime > DateTime.UtcNow));

        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future confirmed bookings.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated member {MemberId}", member.Id);
    }

    public async Task<PaginatedResponse<BookingResponse>> GetMemberBookingsAsync(
        int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            query = query.Where(b => b.Status == parsedStatus);

        if (fromDate.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.ClassSchedule.StartTime) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.ClassSchedule.StartTime) <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = items.Select(MapBookingToResponse).ToList();
        return PaginatedResponse<BookingResponse>.Create(responses, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var now = DateTime.UtcNow;
        var bookings = await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId
                && b.Status == BookingStatus.Confirmed
                && b.ClassSchedule.StartTime > now)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync(ct);

        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<IReadOnlyList<MembershipResponse>> GetMembershipHistoryAsync(int memberId, CancellationToken ct)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == memberId, ct)
            ?? throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var memberships = await db.Memberships
            .AsNoTracking()
            .Include(m => m.MembershipPlan)
            .Include(m => m.Member)
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync(ct);

        return memberships.Select(m => MapMembershipToResponse(m, member)).ToList();
    }

    private static MembershipResponse MapMembershipToResponse(Membership m, Member member) =>
        new(m.Id, m.MemberId, $"{member.FirstName} {member.LastName}",
            m.MembershipPlanId, m.MembershipPlan.Name,
            m.StartDate, m.EndDate, m.Status, m.PaymentStatus,
            m.FreezeStartDate, m.FreezeEndDate, m.CreatedAt, m.UpdatedAt);

    private static BookingResponse MapBookingToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
            b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
            b.CancellationDate, b.CancellationReason,
            b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
            b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);
}
