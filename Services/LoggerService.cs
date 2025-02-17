﻿namespace LoggerBot.Services;

public class LoggerService(IConfiguration configuration) : ILoggerService
{
    private readonly TelegramBotClient botClient = new(configuration["LoggerBot:Token"]!);
    private readonly long chatId = long.Parse(configuration["LoggerBot:ChatId"]!);

    public async Task ErrorAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Error, null, cancellationToken);

    public async Task ErrorAsync(string message, byte[] fileBytes, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Error, fileBytes, cancellationToken);

    public async Task ErrorAsync(Exception exception, CancellationToken cancellationToken = default)
        => await SendMessageAsync(exception, LogType.Error, false, cancellationToken);

    public async Task ErrorAsync(Exception exception, bool detailed = false, CancellationToken cancellationToken = default)
        => await SendMessageAsync(exception, LogType.Error, detailed, cancellationToken);

    public async Task InfoAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Info, null, cancellationToken);

    public async Task SuccessAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Success, null, cancellationToken);

    public async Task WarningAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Warning, null, cancellationToken);

    public async Task MessageAsync(string message, CancellationToken cancellationToken = default)
        => await SendMessageAsync(message, LogType.Message, null, cancellationToken);

    private async Task SendMessageAsync(string text, LogType logType, byte[]? fileBytes = null, CancellationToken cancellationToken = default)
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

    private async Task SendMessageAsync(Exception exception, LogType logType, bool detailed = false, CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            if (detailed)
            {
                var content = GetFullExceptionDetails(exception);
                var bytes = Encoding.UTF8.GetBytes(content);
                using var stream = new MemoryStream(bytes);
                // upload file and reply message to file caption

                Message messageWithFile = await botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: new InputFileStream(stream, "requestData.json"),
                    caption: exception.Message,
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }

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

    public static string GetFullExceptionDetails(Exception ex)
    {
        if (ex == null) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("🔥 Exception Details:");

        CollectExceptionDetails(ex, sb, 0);

        return sb.ToString();
    }

    private static void CollectExceptionDetails(Exception ex, StringBuilder sb, int level)
    {
        if (ex == null) return;

        string indent = new string(' ', level * 4); // Indent inner exceptions
        sb.AppendLine($"{indent}📌 Message: {ex.Message}");
        sb.AppendLine($"{indent}🔍 Type: {ex.GetType().FullName}");
        sb.AppendLine($"{indent}📍 StackTrace: {ex.StackTrace}");

        // Handle AggregateException separately (for Task and Parallel exceptions)
        if (ex is AggregateException aggEx)
        {
            foreach (var inner in aggEx.InnerExceptions)
            {
                sb.AppendLine($"{indent}🔄 Aggregate Inner Exception:");
                CollectExceptionDetails(inner, sb, level + 1);
            }
        }
        else if (ex.InnerException != null)
        {
            sb.AppendLine($"{indent}➡ Inner Exception:");
            CollectExceptionDetails(ex.InnerException, sb, level + 1);
        }
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