using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface IAccountPlanProjectionRepository
{
    Task<AccountPlanProjection?> FindAsync(long ownerUserId, CancellationToken cancellationToken);
    Task AddAsync(AccountPlanProjection projection, CancellationToken cancellationToken);
}
