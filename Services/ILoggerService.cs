namespace LoggerBot.Services;

public interface ILoggerService
{
    Task ErrorAsync(Exception exception);
    Task ErrorAsync(string message);
    Task InfoAsync(string message);
    Task WarningAsync(string message);
    Task SuccessAsync(string message);
    Task MessageAsync(string message);
}