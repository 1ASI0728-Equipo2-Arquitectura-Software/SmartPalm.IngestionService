using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class AccountPlanProjectionRepository(IngestionDbContext context)
    : IAccountPlanProjectionRepository
{
    public Task<AccountPlanProjection?> FindAsync(long ownerUserId, CancellationToken token) =>
        context.AccountPlanProjections.SingleOrDefaultAsync(x => x.OwnerUserId == ownerUserId, token);

    public async Task AddAsync(AccountPlanProjection projection, CancellationToken token) =>
        await context.AccountPlanProjections.AddAsync(projection, token);
}
