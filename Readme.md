# LoggerBot NuGet Package

## Overview
The LoggerBot NuGet package provides a logging service that integrates with Telegram bots. It allows developers to easily log messages of various types (error, info, warning, success, and generic messages) to a designated Telegram chat using a Telegram bot.

## Installation
You can install the LoggerBot NuGet package via the NuGet Package Manager or the .NET CLI:

```bash

dotnet add package LoggerBot

```

## Usage
1. Configure LoggerBot
First, configure LoggerBot in your application's startup code to register the logger service in the dependency injection container:

```csharp

using LoggerBot;

builder.Services.AddLoggerBot();
```

2. Inject and Use LoggerService
Inject the ILoggerService interface into your classes where logging is required and use its methods to log messages:

```csharp

using LoggerBot.Services;

public class MyClass
{
    private readonly ILoggerService _logger;

    public MyClass(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task SomeMethod()
    {
        // Log an error message
        await _logger.ErrorAsync("An error occurred.");

        // Log an info message
        await _logger.InfoAsync("Some information message.");

        // Log a success message
        await _logger.SuccessAsync("Operation completed successfully.");

        // Log a warning message
        await _logger.WarningAsync("Warning: Resource limit exceeded.");

        // Log a generic message
        await _logger.MessageAsync("A generic message.");
    }
}

```

## Configuration
_The LoggerBot requires configuration settings to connect to your Telegram bot. Ensure the following configuration keys are present in your appsettings.json or environment variables:_

**LoggerBot:Token:** The token of your Telegram bot.

**LoggerBot:ChatId:** The ID of the Telegram chat where logs will be sent.

**LoggerBot:TimeZone:** The time zone identifier used to format log timestamps.

Example:
```json

"LoggerBot": {
  "Token": "bot-token",
  "ChatId": "-100chatId",
  "TimeZone": "Asia/Tashkent"
}

```

## Supported Log Types
Error: Used for logging error messages.
Info: Used for logging informational messages.
Success: Used for logging success messages.
Warning: Used for logging warning messages.
Message: Used for logging generic messages.

Feel free to expand upon this documentation with more details specific to your package's usage or additional features!
