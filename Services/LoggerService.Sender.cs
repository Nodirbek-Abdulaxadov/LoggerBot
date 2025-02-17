using Telegram.Bot.Exceptions;

namespace LoggerBot.Services;

public partial class LoggerService
{
    private readonly ConcurrentQueue<LogMessage> _messageQueue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile bool _isProcessing = false;

    private static readonly TimeSpan RateLimitDelay = TimeSpan.FromMilliseconds(35); // 30 msg/sec → ~35ms delay
    private static readonly TimeSpan GroupLimitDelay = TimeSpan.FromSeconds(3); // 20 msg/min → 3 sec delay
    private readonly Dictionary<long, DateTime> _groupLastSentTime = new();

    private void Add(LogMessage message)
    {
        _messageQueue.Enqueue(message);
        StartWorker();
    }

    private void StartWorker()
    {
        if (!_isProcessing && !_messageQueue.IsEmpty)
        {
            _isProcessing = true;
            Task.Run(ProcessQueueAsync);
        }
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            while (_messageQueue.TryDequeue(out var message))
            {
                var delay = GetGroupDelay(message.ChatId);
                if (delay > TimeSpan.Zero) await Task.Delay(delay);

                await SendMessageWithRetry(message);
                _groupLastSentTime[message.ChatId] = DateTime.UtcNow;

                await Task.Delay(RateLimitDelay);
            }
        }
        finally
        {
            _isProcessing = false;

            if (!_messageQueue.IsEmpty)
            {
                StartWorker();
            }
        }
    }

    private async Task SendMessageWithRetry(LogMessage message)
    {
        int retryCount = 0;

        while (retryCount < 5)
        {
            try
            {
                await SendMessage(message);
                return;
            }
            catch (ApiRequestException ex) when (ex.ErrorCode == 429)
            {
                int retryAfter = ex.Parameters?.RetryAfter ?? 5;
                await Task.Delay(TimeSpan.FromSeconds(retryAfter));
                retryCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send message: {ex.Message}");
                return;
            }
        }

        Console.WriteLine("[ERROR] Max retries reached. Dropping message.");
    }

    private async Task SendMessage(LogMessage message)
    {
        if (message.HasDocument)
        {
            using var stream = new MemoryStream(message.FileBytes!);
            await botClient.SendDocumentAsync(
                chatId: message.ChatId,
                document: new InputFileStream(stream, "details.json"),
                caption: message.Text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                disableNotification: true);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: message.ChatId,
                text: message.Text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                disableNotification: true);
        }
    }

    private TimeSpan GetGroupDelay(long chatId)
    {
        if (_groupLastSentTime.TryGetValue(chatId, out var lastSent))
        {
            var elapsed = DateTime.UtcNow - lastSent;
            return elapsed < GroupLimitDelay ? GroupLimitDelay - elapsed : TimeSpan.Zero;
        }

        return TimeSpan.Zero;
    }
}
