namespace SmartPalm.IngestionService.API.Tests.Integration;
public class DockerRequiredFactAttribute : FactAttribute
{
    public DockerRequiredFactAttribute() { if (string.Equals(Environment.GetEnvironmentVariable("SKIP_DOCKER_TESTS"), "true", StringComparison.OrdinalIgnoreCase)) Skip = "Docker tests disabled."; }
}
