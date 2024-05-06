using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LoggerBot;

internal static class Log
{
    static TelegramBotClient botClient = new ("6523591589:AAEOMg2NgjRqCMvtoRMfjycYHcTKl3Zj3wc");
    static long chatId = -1002069993572;
    public static async Task Error(string text)
    {
        await Task.Run(async () =>
        {
            var today = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"[❌ERROR] {today}:\n\n{text}",
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    public static async Task Info(string text)
    {
        await Task.Run(async () =>
        {
            var today = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"[ℹ️INFO] {today}:\n\n{text}",
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    public static async Task Warning(string text)
    {
        await Task.Run(async () =>
        {
            var today = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"[⚠️WARNING] {today}:\n\n{text}",
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    public static async Task Success(string text)
    {
        await Task.Run(async () =>
        {
            var today = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"[✅SUCCESS] {today}:\n\n{text}",
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }

    public static async Task Message(string text)
    {
        await Task.Run(async () =>
        {
            var today = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            Message message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"[📩MESSAGE] {today}:\n\n{text}",
            parseMode: ParseMode.Markdown,
            disableNotification: true);
        }).ConfigureAwait(false);
    }
}