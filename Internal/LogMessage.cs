namespace LoggerBot.Internal;

internal readonly struct LogMessage(long chatId,
                                    string text,
                                    bool hasDocument = false,
                                    byte[]? fileBytes = null,
                                    CancellationToken cancellationToken = default)
{
    public long ChatId { get; } = chatId;
    public string Text { get; } = text;
    public bool HasDocument { get; } = hasDocument;
    public byte[]? FileBytes { get; } = fileBytes;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}