namespace SmartPalm.IngestionService.Application.Abstractions;

public interface IIntegrationEventWriter
{
    void Enqueue<T>(string eventType, T payload) where T : notnull;
}
