using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IAttendeeService
{
    Task<PaginatedList<Attendee>> GetAllAsync(string? search, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Attendee>> GetAllForDropdownAsync(CancellationToken ct = default);
    Task<Attendee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Attendee?> GetByIdWithRegistrationsAsync(int id, CancellationToken ct = default);
    Task<Attendee> CreateAsync(Attendee attendee, CancellationToken ct = default);
    Task UpdateAsync(Attendee attendee, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
    Task<Attendee?> GetByEmailAsync(string email, CancellationToken ct = default);
}
