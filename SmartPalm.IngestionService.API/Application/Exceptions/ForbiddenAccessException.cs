namespace SmartPalm.IngestionService.Application.Exceptions;

public sealed class ForbiddenAccessException(string message) : Exception(message);
