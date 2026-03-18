using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Members.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.FirstName.Contains(search) ||
                m.LastName.Contains(search) ||
                m.Email.Contains(search));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => MapToResponse(m))
            .ToListAsync(ct);

        return PaginatedResponse<MemberResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<MemberDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        if (member is null) return null;

        var activeMembership = await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == id && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen))
            .FirstOrDefaultAsync(ct);

        var totalBookings = await db.Bookings.AsNoTracking().CountAsync(b => b.MemberId == id, ct);
        var upcomingBookings = await db.Bookings.AsNoTracking()
            .Where(b => b.MemberId == id && b.Status == BookingStatus.Confirmed)
            .Join(db.ClassSchedules, b => b.ClassScheduleId, cs => cs.Id, (b, cs) => cs.StartTime)
            .CountAsync(st => st > DateTime.UtcNow, ct);

        return new MemberDetailResponse
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
            TotalBookings = totalBookings,
            UpcomingBookings = upcomingBookings,
            ActiveMembership = activeMembership is null ? null : MapMembershipToResponse(activeMembership)
        };
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
    {
        if (await db.Members.AnyAsync(m => m.Email == request.Email, ct))
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var age = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - request.DateOfBirth.DayNumber;
        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16)))
            throw new ArgumentException("Member must be at least 16 years old.");

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

        logger.LogInformation("Registered member {MemberEmail} with ID {MemberId}", member.Email, member.Id);
        return MapToResponse(member);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return null;

        if (await db.Members.AnyAsync(m => m.Email == request.Email && m.Id != id, ct))
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
        return MapToResponse(member);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([id], ct);
        if (member is null) return false;

        var hasFutureBookings = await db.Bookings.AsNoTracking()
            .Where(b => b.MemberId == id && b.Status == BookingStatus.Confirmed)
            .Join(db.ClassSchedules, b => b.ClassScheduleId, cs => cs.Id, (b, cs) => cs.StartTime)
            .AnyAsync(st => st > DateTime.UtcNow, ct);

        if (hasFutureBookings)
            throw new InvalidOperationException("Cannot deactivate member with future bookings. Cancel bookings first.");

        member.IsActive = false;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated member {MemberId}", member.Id);
        return true;
    }

    public async Task<PaginatedResponse<BookingResponse>> GetBookingsAsync(int memberId, string? status, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var query = db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var bs))
            query = query.Where(b => b.Status == bs);

        if (from.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.BookingDate) >= from.Value);

        if (to.HasValue)
            query = query.Where(b => DateOnly.FromDateTime(b.BookingDate) <= to.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PaginatedResponse<BookingResponse>.Create(
            items.Select(MapBookingToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<List<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var bookings = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Confirmed && b.ClassSchedule.StartTime > DateTime.UtcNow)
            .OrderBy(b => b.ClassSchedule.StartTime)
            .ToListAsync(ct);

        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<List<MembershipResponse>> GetMembershipsAsync(int memberId, CancellationToken ct)
    {
        if (!await db.Members.AnyAsync(m => m.Id == memberId, ct))
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var memberships = await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .Include(ms => ms.Member)
            .Where(ms => ms.MemberId == memberId)
            .OrderByDescending(ms => ms.StartDate)
            .ToListAsync(ct);

        return memberships.Select(MapMembershipToResponse).ToList();
    }

    private static MemberResponse MapToResponse(Member member) => new()
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
        UpdatedAt = member.UpdatedAt
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
        ClassStartTime = b.ClassSchedule?.StartTime ?? default,
        ClassEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? string.Empty,
        InstructorName = b.ClassSchedule?.Instructor is not null
            ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}"
            : string.Empty,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
