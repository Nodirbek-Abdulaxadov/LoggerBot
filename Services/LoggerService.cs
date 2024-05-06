namespace LoggerBot.Services;

public class LoggerService(IConfiguration configuration) : ILoggerService
{
    private readonly TelegramBotClient botClient = new(configuration["LoggerBot:Token"]!);
    private readonly long chatId = long.Parse(configuration["LoggerBot:ChatId"]!);

    public async Task ErrorAsync(string message)
        => await SendMessageAsync(message, LogType.Error);

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
            text = logType switch 
            {
                LogType.Error => $"**[❌ERROR]** {Now()}:\n\n{text}",
                LogType.Info => $"**[ℹ️INFO]** {Now()}:\n\n{text}",
                LogType.Warning => $"**[⚠️WARNING]** {Now()}:\n\n{text}",
                LogType.Success => $"**[✅SUCCESS]** {Now()}:\n\n{text}",
                LogType.Message => $"**[📩MESSAGE]** {Now()}:\n\n{text}",
                _ => $"**[📩MESSAGE]** {Now()}:\n\n{text}"
            };

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    private string Now()
    {
        var utcNow = DateTime.UtcNow;
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["LoggerBot:TimeZone"]!);
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone).ToString("HH:mm:ss dd/MM/yyyy");
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