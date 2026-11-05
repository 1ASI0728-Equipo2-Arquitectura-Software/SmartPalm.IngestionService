using Microsoft.EntityFrameworkCore;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

public static class IngestionDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IngestionDatabaseInitializer");
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try { await db.Database.MigrateAsync(); return; }
            catch (Exception ex) when (attempt < 10) { logger.LogWarning(ex, "Migration attempt {Attempt} failed; retrying.", attempt); await Task.Delay(TimeSpan.FromSeconds(2)); }
        }
    }
}
