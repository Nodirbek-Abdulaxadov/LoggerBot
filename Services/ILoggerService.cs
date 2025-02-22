﻿namespace LoggerBot.Services;

public interface ILoggerService
{
    Task ErrorAsync(Exception exception, string? projectName = null, bool detailed = false, CancellationToken cancellationToken = default);
    Task ErrorMessageAsync(string message, string? projectName = null, CancellationToken cancellationToken = default);
    Task ErrorAttachmentAsync(string message, byte[] fileBytes, string? projectName = null, CancellationToken cancellationToken = default);
    Task InfoAsync(string message, string? projectName = null, CancellationToken cancellationToken = default);
    Task WarningAsync(string message, string? projectName = null, CancellationToken cancellationToken = default);
    Task SuccessAsync(string message, string? projectName = null, CancellationToken cancellationToken = default);
    Task MessageAsync(string message, string? projectName = null, CancellationToken cancellationToken = default);
}