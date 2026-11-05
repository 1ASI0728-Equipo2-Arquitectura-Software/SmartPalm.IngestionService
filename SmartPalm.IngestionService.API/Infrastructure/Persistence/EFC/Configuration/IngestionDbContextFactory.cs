using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

public class IngestionDbContextFactory : IDesignTimeDbContextFactory<IngestionDbContext>
{
    public IngestionDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<IngestionDbContext>()
        .UseNpgsql(GetConnectionString())
        .UseSnakeCaseNamingConvention().Options);

    private static string GetConnectionString() => BuildConfiguration().GetConnectionString("IngestionDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:IngestionDatabase is required.");

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .SetBasePath(ResolveConfigurationPath())
            .AddJsonFile("appsettings.json", optional: false);
        if (!string.IsNullOrWhiteSpace(environment)) builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        return builder.AddEnvironmentVariables().Build();
    }

    private static string ResolveConfigurationPath()
    {
        foreach (var path in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            if (File.Exists(Path.Combine(path, "appsettings.json"))) return path;
        throw new FileNotFoundException("Unable to locate appsettings.json for EF Core design-time configuration.");
    }
}
