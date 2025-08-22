namespace LoggerBot.Internal;

internal readonly struct LogMessage
{
    public long ChatId { get; } 
    public string Text { get; }
    public bool HasDocument { get; } 
    public byte[]? FileBytes { get; }
    public CancellationToken CancellationToken { get; }

    public LogMessage(long chatId,
                      string text,
                      bool hasDocument = false,
                      byte[]? fileBytes = null,
                      CancellationToken cancellationToken = default)
    {
        ChatId = chatId;
        Text = text;
        HasDocument = hasDocument;
        FileBytes = fileBytes;
        CancellationToken = cancellationToken;
    }
}