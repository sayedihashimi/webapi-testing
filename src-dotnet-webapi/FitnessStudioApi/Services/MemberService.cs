using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PagedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        logger.LogInformation("Fetching members page={Page} pageSize={PageSize}", page, pageSize);
        var query = db.Members.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(m => m.FirstName.ToLower().Contains(s) || m.LastName.ToLower().Contains(s) || m.Email.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapToResponse(m))
            .ToListAsync(ct);

        return PagedResponse<MemberResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        logger.LogInformation("Fetching member {MemberId}", id);
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        if (member is null) return null;

        var activeMembership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

        var bookings = await db.Bookings.AsNoTracking().Where(b => b.MemberId == id).ToListAsync(ct);

        var detail = new MemberDetailResponse
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
            ActiveMembership = activeMembership is null ? null : MapMembershipToResponse(activeMembership),
            BookingStats = new MemberBookingStats
            {
                TotalBookings = bookings.Count,
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                AttendedBookings = bookings.Count(b => b.Status == BookingStatus.Attended),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                NoShowBookings = bookings.Count(b => b.Status == BookingStatus.NoShow)
            }
        };

        return detail;
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        var emailExists = await db.Members.AnyAsync(m => m.Email == request.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var minAge = DateOnly.FromDateTime(DateTime.Today).AddYears(-16);
        if (request.DateOfBirth > minAge)
            throw new ArgumentException("Member must be at least 16 years old.");

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            JoinDate = DateOnly.FromDateTime(DateTime.Today)
        };

        db.Members.Add(member);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created member {MemberId} '{Email}'", member.Id, member.Email);
        return MapToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return null;

        var emailConflict = await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.EmergencyContactName = request.EmergencyContactName;
        member.EmergencyContactPhone = request.EmergencyContactPhone;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated member {MemberId}", id);
        return MapToResponse(member);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return (false, "Member not found.");

        var hasFutureBookings = await db.Bookings.AnyAsync(
            b => b.MemberId == id && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow, ct);
        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future confirmed bookings. Cancel bookings first.");

        member.IsActive = false;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deactivated member {MemberId}", id);
        return (true, null);
    }

    public async Task<PagedResponse<BookingResponse>> GetMemberBookingsAsync(int memberId, string? status, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists) throw new KeyNotFoundException($"Member {memberId} not found.");

        var query = db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (fromDate.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.ClassSchedule.StartTime) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.ClassSchedule.StartTime) <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);

        return PagedResponse<BookingResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists) throw new KeyNotFoundException($"Member {memberId} not found.");

        return await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .Select(b => MapBookingToResponse(b))
            .ToListAsync(ct);
    }

    public async Task<List<MembershipResponse>> GetMemberMembershipsAsync(int memberId, CancellationToken ct)
    {
        var memberExists = await db.Members.AnyAsync(m => m.Id == memberId, ct);
        if (!memberExists) throw new KeyNotFoundException($"Member {memberId} not found.");

        return await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .Select(ms => MapMembershipToResponse(ms))
            .ToListAsync(ct);
    }

    private static MemberResponse MapToResponse(Member m) => new()
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

    private static MembershipResponse MapMembershipToResponse(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = ms.Member is not null ? $"{ms.Member.FirstName} {ms.Member.LastName}" : string.Empty,
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan?.Name ?? string.Empty,
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status,
        PaymentStatus = ms.PaymentStatus,
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };

    private static BookingResponse MapBookingToResponse(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? string.Empty,
        MemberId = b.MemberId,
        MemberName = b.Member is not null ? $"{b.Member.FirstName} {b.Member.LastName}" : string.Empty,
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        ScheduleStartTime = b.ClassSchedule?.StartTime ?? default,
        ScheduleEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? string.Empty,
        InstructorName = b.ClassSchedule?.Instructor is not null ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}" : string.Empty,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
