using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Services;

namespace SmartPalm.IngestionService.API.Tests.Domain;

public class AgronomicThresholdTests
{
    [Fact]
    public void Threshold_EvaluatesBoundaryAndExceededValues()
    {
        var threshold = new AgronomicThreshold("AA", "BB", SensorType.Temperature, 10, 40, "Temperature");
        Assert.False(threshold.IsExceededBy(10));
        Assert.False(threshold.IsExceededBy(40));
        Assert.True(threshold.IsExceededBy(40.1));
    }

    [Fact]
    public void Update_RejectsInvertedRange()
    {
        var threshold = new AgronomicThreshold("AA", "BB", SensorType.Humidity, 0, 100, "Humidity");
        Assert.Throws<ArgumentException>(() => threshold.Update(101, null, null));
    }

    [Fact]
    public void DefaultPolicy_CreatesAllFiveSensorTypes() =>
        Assert.Equal(5, ThresholdPolicy.CreateDefaults("AA", "BB").Select(x => x.Type).Distinct().Count());
}
