using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface IIngestionBatchRepository
{
    Task<bool> ExistsAsync(string fingerprint, CancellationToken cancellationToken);
    Task AddAsync(IngestionBatch batch, CancellationToken cancellationToken);
}
