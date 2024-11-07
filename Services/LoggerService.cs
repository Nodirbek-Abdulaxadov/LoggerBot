namespace LoggerBot.Services;

public class LoggerService(IConfiguration configuration) : ILoggerService
{
    private readonly TelegramBotClient botClient = new(configuration["LoggerBot:Token"]!);
    private readonly long chatId = long.Parse(configuration["LoggerBot:ChatId"]!);

    public async Task ErrorAsync(string message)
        => await SendMessageAsync(message, LogType.Error);

    public async Task ErrorAsync(Exception exception)
        => await SendMessageAsync(exception, LogType.Error);

    public async Task InfoAsync(string message)
        => await SendMessageAsync(message, LogType.Info);

    public async Task SuccessAsync(string message)
        => await SendMessageAsync(message, LogType.Success);

    public async Task WarningAsync(string message)
        => await SendMessageAsync(message, LogType.Warning);

    public async Task MessageAsync(string message)
        => await SendMessageAsync(message, LogType.Message);

    private async Task SendMessageAsync(string text, LogType logType)
    {
        await Task.Run(async () =>
        {
            var now = DateTime.Now;
            text = logType switch 
            {
                LogType.Error => $"**[❌ERROR]** {now}:\n\n{text}",
                LogType.Info => $"**[ℹ️INFO]** {now}:\n\n{text}",
                LogType.Warning => $"**[⚠️WARNING]** {now}:\n\n{text}",
                LogType.Success => $"**[✅SUCCESS]** {now}:\n\n{text}",
                LogType.Message => $"**[📩MESSAGE]** {now}:\n\n{text}",
                _ => $"**[📩MESSAGE]** {now}:\n\n{text}"
            };

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    private async Task SendMessageAsync(Exception exception, LogType logType)
    {
        await Task.Run(async () =>
        {
            string source = exception.StackTrace?.Split("\n")[0] ?? "";
            string text = $"""
            **[❌ERROR]** {DateTime.Now}:
            
            🛑{exception.GetType().Name}: {exception.Message}
            Inner Exception: {exception.InnerException?.Message}
            
            🪲Source: {source}
            """;

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    private enum LogType
    {
        Error,
        Info,
        Warning,
        Success,
        Message
    }
}