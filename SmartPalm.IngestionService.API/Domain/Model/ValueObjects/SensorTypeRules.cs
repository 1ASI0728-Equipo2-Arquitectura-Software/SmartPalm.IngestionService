namespace SmartPalm.IngestionService.Domain.Readings;

public static class SensorTypeRules
{
    public static MeasureUnit UnitFor(SensorType type) => type switch
    {
        SensorType.Humidity or SensorType.SoilMoisture => MeasureUnit.Percent,
        SensorType.Temperature => MeasureUnit.Celsius,
        SensorType.Luminosity => MeasureUnit.Lux,
        SensorType.PH => MeasureUnit.Ph,
        _ => MeasureUnit.Unknown
    };
}
