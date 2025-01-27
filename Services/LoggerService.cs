using System.Diagnostics;
using System.Text;

namespace LoggerBot.Services;

public class LoggerService(IConfiguration configuration) : ILoggerService
{
    private readonly TelegramBotClient botClient = new(configuration["LoggerBot:Token"]!);
    private readonly long chatId = long.Parse(configuration["LoggerBot:ChatId"]!);

    public async Task ErrorAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Error, cancellationToken);

    public async Task ErrorAsync(string message, byte[] fileBytes, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Error, cancellationToken, fileBytes);

    public async Task ErrorAsync(Exception exception, CancellationToken cancellationToken = default)
        => await SendMessageAsync(exception, LogType.Error, cancellationToken);

    public async Task InfoAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Info, cancellationToken);

    public async Task SuccessAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Success, cancellationToken);

    public async Task WarningAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Warning, cancellationToken);

    public async Task MessageAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Message, cancellationToken);

    private async Task SendMessageAsync(string text, LogType logType, CancellationToken cancellationToken = default, byte[]? fileBytes = null)
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

            if (fileBytes is not null)
            {
                using var stream = new MemoryStream(fileBytes);
                // upload file and reply message to file caption

                Message messageWithFile = await botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: new InputFileStream(stream, "requestData.json"),
                    caption: text,
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Markdown,
            disableNotification: true,
            cancellationToken: cancellationToken);
        }).ConfigureAwait(false);
    }

    private async Task SendMessageAsync(Exception exception, LogType logType, CancellationToken cancellationToken = default)
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
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env))
            {
                stringBuilder.AppendLine($"Environment: {env}");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"\U0001f6d1{exception.GetType().Name}: {exception.Message}");
            if (exception.InnerException is not null)
            {
                stringBuilder.AppendLine($"Inner Exception: {exception.InnerException?.Message}");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"\U0001fab2Source: {sourceLines}");

            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: stringBuilder.ToString(),
            parseMode: ParseMode.Markdown,
            disableNotification: true,
            cancellationToken: cancellationToken);
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