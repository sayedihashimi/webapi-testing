using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ILeaseService
{
    Task<PaginatedList<Lease>> GetLeasesAsync(LeaseStatus? status, int? propertyId, string? search, int pageNumber, int pageSize);
    Task<Lease?> GetByIdAsync(int id);
    Task<Lease?> GetWithDetailsAsync(int id);
    Task<(bool Success, string? Error)> CreateAsync(Lease lease);
    Task<(bool Success, string? Error)> UpdateAsync(Lease lease);
    Task<(bool Success, string? Error)> TerminateAsync(int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount);
    Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(int id, DateOnly newEndDate, decimal newRentAmount);
    Task<(bool Success, string? Error)> ActivateAsync(int id);
    Task<List<Lease>> GetExpiringLeasesAsync(int days);
    Task<bool> HasOverlappingLeaseAsync(int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId = null);
}
