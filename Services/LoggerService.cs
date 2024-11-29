using System.Diagnostics;
using System.Text;

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
            var stackTrace = new StackTrace(exception, true);
            string sourceLines = "";

            // Include up to 5 lines of source information if available
            var frames = stackTrace.GetFrames();
            if (frames != null)
            {
                for (int i = 0; i < Math.Min(10, frames.Length); i++)
                {
                    var frame = frames[i];
                    var fileName = frame.GetFileName();
                    var lineNumber = frame.GetFileLineNumber();
                    var methodName = frame.GetMethod()?.Name;

                    if (!string.IsNullOrWhiteSpace(fileName) && lineNumber > 0)
                    {
                        sourceLines += $"\n    At {fileName}:{lineNumber} in {methodName}";
                    }
                }
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"**[❌ERROR]** {DateTime.Now}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"\U0001f6d1{exception.GetType().Name}: {exception.Message}");
            if(exception.InnerException is not null)
            {
                stringBuilder.AppendLine($"Inner Exception: {exception.InnerException?.Message}");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"\U0001fab2Source: {sourceLines}");

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: stringBuilder.ToString(),
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