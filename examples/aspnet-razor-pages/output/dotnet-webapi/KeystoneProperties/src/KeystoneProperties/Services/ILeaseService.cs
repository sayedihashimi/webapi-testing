using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ILeaseService
{
    Task<PaginatedList<Lease>> GetAllAsync(LeaseStatus? status, int? propertyId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Lease?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(Lease? Lease, string? Error)> CreateAsync(Lease lease, CancellationToken ct = default);
    Task<string?> UpdateAsync(Lease lease, CancellationToken ct = default);
    Task<(bool Success, string? Error)> TerminateAsync(int id, DateOnly terminationDate, string reason, DepositStatus depositStatus, decimal? depositReturnAmount, CancellationToken ct = default);
    Task<(Lease? Lease, string? Error)> RenewAsync(int originalLeaseId, DateOnly newEndDate, decimal newMonthlyRent, CancellationToken ct = default);
    Task<List<Lease>> GetActiveLeasesByUnitAsync(int unitId, CancellationToken ct = default);
    Task<List<Lease>> GetActiveLeasesAsync(CancellationToken ct = default);
}
