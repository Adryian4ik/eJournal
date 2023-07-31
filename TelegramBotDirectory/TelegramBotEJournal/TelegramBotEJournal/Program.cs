using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotEJournal;

try
{
    var botClient = new TelegramBotClient("6238204903:AAGC0cSEzOb1w5_zpAC1OmB--ZcoBjNAe4s");

    using CancellationTokenSource cts = new ();

    // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
    ReceiverOptions receiverOptions = new ()
    {
        AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
    };

    botClient.StartReceiving(
                             updateHandler: HandleUpdateAsync,
                             pollingErrorHandler: HandlePollingErrorAsync,
                             receiverOptions: receiverOptions,
                             cancellationToken: cts.Token
                            );
    
    await Task.Delay(-1);
    
    //string consoleInput;
    /*while ((consoleInput = Console.ReadLine()).ToLower() != "exit")
    {
        switch (consoleInput)
        {
            default:
                break;
        }
    }*/

    // Send cancellation request to stop bot
    cts.Cancel();

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        Console.WriteLine($"{message.Chat.Id}: {message.From.Id} {message.From.Username}: {messageText}");
        
        try
        {
            var result = await HostApiAccess.GetStudent(message.From.Id);
            // Echo received message text
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                               text: $"You said: {messageText}\n{result}",
                                               cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                 text: $"You said: {messageText}\n{e}",
                                                 cancellationToken: cancellationToken);
        }
        
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
