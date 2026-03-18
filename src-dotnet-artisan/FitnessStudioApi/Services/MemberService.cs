using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MemberService(StudioDbContext db)
{
    public async Task<PagedResponse<MemberResponse>> GetAllAsync(
        string? search, bool? isActive, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = db.Members.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var members = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var memberIds = members.Select(m => m.Id).ToList();
        var activeMemberships = await db.Memberships
            .Where(ms => memberIds.Contains(ms.MemberId) &&
                         (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .Include(ms => ms.MembershipPlan)
            .ToListAsync(ct);

        var responses = members.Select(m =>
        {
            var activeMembership = activeMemberships.FirstOrDefault(ms => ms.MemberId == m.Id);
            return ToResponse(m, activeMembership);
        }).ToList();

        return new PagedResponse<MemberResponse>(
            responses, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        var activeMembership = await db.Memberships
            .Where(ms => ms.MemberId == id &&
                         (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ct);

        return ToResponse(member, activeMembership);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        if (await db.Members.AnyAsync(m => m.Email == request.Email, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        var minBirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-16);
        if (request.DateOfBirth > minBirthDate)
        {
            throw new InvalidOperationException("Member must be at least 16 years old.");
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
        return ToResponse(member, null);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return null;
        }

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
        {
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");
        }

        var minBirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-16);
        if (request.DateOfBirth > minBirthDate)
        {
            throw new InvalidOperationException("Member must be at least 16 years old.");
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

        var activeMembership = await db.Memberships
            .Where(ms => ms.MemberId == id &&
                         (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ct);

        return ToResponse(member, activeMembership);
    }

    public async Task<string?> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null)
        {
            return "not_found";
        }

        var hasFutureBookings = await db.Bookings
            .AnyAsync(b => b.MemberId == id &&
                          b.Status == BookingStatus.Confirmed &&
                          b.ClassSchedule.StartTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
        {
            return "has_bookings";
        }

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<PagedResponse<BookingResponse>> GetBookingsAsync(
        int memberId, string? status, DateTime? from, DateTime? to,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = db.Bookings
            .Where(b => b.MemberId == memberId)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
        {
            query = query.Where(b => b.Status == bs);
        }

        if (from.HasValue)
        {
            query = query.Where(b => b.ClassSchedule.StartTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.ClassSchedule.StartTime <= to.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var bookings = await query
            .OrderByDescending(b => b.ClassSchedule.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = bookings.Select(BookingService.ToResponse).ToList();
        return new PagedResponse<BookingResponse>(
            items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct = default)
    {
        var bookings = await db.Bookings
            .Where(b => b.MemberId == memberId &&
                       b.Status == BookingStatus.Confirmed &&
                       b.ClassSchedule.StartTime > DateTime.UtcNow)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync(ct);

        return bookings.Select(BookingService.ToResponse).ToList();
    }

    public async Task<List<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct = default)
    {
        var memberships = await db.Memberships
            .Where(ms => ms.MemberId == memberId)
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync(ct);

        return memberships.Select(MembershipService.ToResponse).ToList();
    }

    private static MemberResponse ToResponse(Member m, Membership? activeMembership) => new(
        m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.DateOfBirth,
        m.EmergencyContactName, m.EmergencyContactPhone, m.JoinDate, m.IsActive,
        activeMembership is not null
            ? new MembershipSummary(
                activeMembership.Id,
                activeMembership.MembershipPlan.Name,
                activeMembership.Status.ToString(),
                activeMembership.StartDate,
                activeMembership.EndDate)
            : null);
}
