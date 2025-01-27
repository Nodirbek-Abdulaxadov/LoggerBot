namespace LoggerBot.Services;

public interface ILoggerService
{
    Task ErrorAsync(Exception exception, CancellationToken cancellationToken = default);
    Task ErrorAsync(string message, CancellationToken cancellationToken = default);
    Task ErrorAsync(string message, byte[] fileBytes, CancellationToken cancellationToken = default);
    Task InfoAsync(string message, CancellationToken cancellationToken = default);
    Task WarningAsync(string message, CancellationToken cancellationToken = default);
    Task SuccessAsync(string message, CancellationToken cancellationToken = default);
    Task MessageAsync(string message, CancellationToken cancellationToken = default);
}