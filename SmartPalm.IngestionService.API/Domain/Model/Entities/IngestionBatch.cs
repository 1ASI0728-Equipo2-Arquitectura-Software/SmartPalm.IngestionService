namespace SmartPalm.IngestionService.Domain.Readings;

public class IngestionBatch
{
    private IngestionBatch() { }
    public IngestionBatch(string fingerprint, string gatewayMacAddress, DateTime synchronizedAt, int readingCount)
    {
        if (string.IsNullOrWhiteSpace(fingerprint)) throw new ArgumentException("Fingerprint is required.", nameof(fingerprint));
        if (readingCount <= 0) throw new ArgumentOutOfRangeException(nameof(readingCount));
        Id = Guid.NewGuid();
        Fingerprint = fingerprint;
        GatewayMacAddress = SensorReading.NormalizeIdentifier(gatewayMacAddress);
        SynchronizedAt = synchronizedAt.Kind == DateTimeKind.Utc ? synchronizedAt : synchronizedAt.ToUniversalTime();
        ReadingCount = readingCount;
        ReceivedAt = DateTime.UtcNow;
    }
    public Guid Id { get; private set; }
    public string Fingerprint { get; private set; } = string.Empty;
    public string GatewayMacAddress { get; private set; } = string.Empty;
    public DateTime SynchronizedAt { get; private set; }
    public int ReadingCount { get; private set; }
    public DateTime ReceivedAt { get; private set; }
}
