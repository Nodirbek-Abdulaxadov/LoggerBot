namespace LoggerBot;

public static class Configure
{
    public static void AddLoggerBot(this IServiceCollection services)
        => services.AddSingleton<ILoggerService, LoggerService>();
}