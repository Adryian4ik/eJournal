using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotEJournal;
using TelegramBotEJournal.Commands;

try
{
    var botClient = new TelegramBotClient("6238204903:AAGC0cSEzOb1w5_zpAC1OmB--ZcoBjNAe4s");
    
    using CancellationTokenSource cts = new ();

    Console.WriteLine("Настройка кнопки Меню...");
    await botClient.SetChatMenuButtonAsync(null, new MenuButtonCommands());
    
    Console.WriteLine("Успешно!\nНастройка команд бота...");

    List<TelegramBotCommand> commands = new List<TelegramBotCommand>();
    commands.Add(new TelegramBotCommand(botClient, "me", "Краткая информация о студенте", async (client, update) =>
    {
        try
        {
            await client.SendTextMessageAsync(update.Message.Chat.Id,
                                              (await HostApiAccess.GetStudent(update.Message.Chat.Id)).ToString(),
                                              cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }));
    
    commands.Add(new TelegramBotCommand(botClient, "todaypasses", "Информация о сегодняшних пропусках",
                                        async (client, update) =>
                                        {
                                            try
                                            {
                                                var student = await HostApiAccess.GetStudentIDFromTelegramID(update.Message.Chat.Id);
                                                if (student == -1) return false;
                                                var passes = await HostApiAccess.GetPasses(student, DateTime.Now);
                                                string answer = string.Join("\n\n", passes.Select(x => x.ToString()));

                                                if (string.IsNullOrWhiteSpace(answer)) answer = "Пропусков нет";
                                                
                                                await client.SendTextMessageAsync(update.Message.Chat.Id,
                                                 answer,
                                                 cancellationToken: cts.Token);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                                return false;
                                            }

                                            return true;
                                        }));
    
    await botClient.SetMyCommandsAsync(commands.Select(x => x.Command));
    Console.WriteLine("Успешно!");

    // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
    ReceiverOptions receiverOptions = new ()
    {
        AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
    };

    botClient.StartReceiving(updateHandler: HandleUpdateAsync,
                             pollingErrorHandler: HandlePollingErrorAsync,
                             receiverOptions: receiverOptions,
                             cancellationToken: cts.Token
                            );

    while (true)
    {
        await Task.Delay(-1);
    }

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

        Console.WriteLine($"{message.Chat.Id}: {message.From?.Id} {message.From?.Username}: {messageText}");
        
        if (message.Entities != null)
            foreach (var entity in message.Entities)
            {
                if (entity.Type == MessageEntityType.BotCommand)
                {
                    if (messageText[0] == '/')
                    {
                        var commandText = string.Join("", messageText.Skip(1));
                        foreach (var command in commands)
                        {
                            if (command.Command.Command.Equals(commandText))
                            {
                                Console.WriteLine(await command.Execute(update));
                                return;
                            }
                        }
                    }
                }
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
