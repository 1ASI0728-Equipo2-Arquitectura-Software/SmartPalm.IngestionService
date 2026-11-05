using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class IngestionBatchRepository(IngestionDbContext context) : IIngestionBatchRepository
{
    public Task<bool> ExistsAsync(string fingerprint, CancellationToken token) => context.IngestionBatches.AnyAsync(x => x.Fingerprint == fingerprint, token);
    public async Task AddAsync(IngestionBatch batch, CancellationToken token) => await context.IngestionBatches.AddAsync(batch, token);
}
