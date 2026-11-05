using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.API.Tests.Domain;

public class SensorReadingTests
{
    [Fact]
    public void Constructor_NormalizesIdentifiersAndInfersUnit()
    {
        var reading = new SensorReading(Guid.NewGuid(), " aa:bb ", " smart-palm-001 ", SensorType.Temperature, DateTime.UtcNow, 28.4);
        Assert.Equal("AA:BB", reading.GatewayMacAddress);
        Assert.Equal("SMART-PALM-001", reading.IotDeviceMacAddress);
        Assert.Equal(MeasureUnit.Celsius, reading.Unit);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsNonFiniteValues(double value) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new SensorReading(Guid.NewGuid(), "AA", "BB", SensorType.Humidity, DateTime.UtcNow, value));
}
