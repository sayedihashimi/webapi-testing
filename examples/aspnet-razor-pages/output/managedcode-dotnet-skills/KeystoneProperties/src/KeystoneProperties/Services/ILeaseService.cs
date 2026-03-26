using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ILeaseService
{
    Task<PaginatedList<Lease>> GetLeasesAsync(LeaseStatus? status, int? propertyId, int pageNumber, int pageSize);
    Task<Lease?> GetLeaseByIdAsync(int id);
    Task<Lease?> GetLeaseWithDetailsAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateLeaseAsync(Lease lease);
    Task<(bool Success, string? ErrorMessage)> UpdateLeaseAsync(Lease lease);
    Task<(bool Success, string? ErrorMessage)> TerminateLeaseAsync(int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount);
    Task<(bool Success, string? ErrorMessage)> RenewLeaseAsync(int originalLeaseId, DateOnly newEndDate, decimal newMonthlyRent);
    Task<List<Lease>> GetExpiringLeasesAsync(int days);
    Task<Lease?> GetActiveLeaseForUnitAsync(int unitId);
    Task<bool> HasOverlappingLeaseAsync(int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId = null);
}
