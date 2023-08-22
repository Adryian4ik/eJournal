using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotEJournal;
using TelegramBotEJournal.Commands;
using TelegramBotEJournal.Entities;

try
{
    var tokenPath = Path.Combine(Environment.CurrentDirectory, "Token");
    if (!System.IO.File.Exists(tokenPath) || string.IsNullOrWhiteSpace(System.IO.File.ReadAllText(tokenPath)))
    {
        Console.Write("Введите токен бота: ");
        var token = Console.ReadLine();
        System.IO.File.WriteAllText(tokenPath, token);
    }
    
    Console.WriteLine("Авторизация бота...");
    
    var botClient = new TelegramBotClient(System.IO.File.ReadAllText(tokenPath));
    if (!await botClient.TestApiAsync())
    {
        Console.WriteLine("Проверьте правильность введенного токена в файле 'Token'");
        return;
    }
    Console.WriteLine("Успешно!");
    
    using CancellationTokenSource cts = new ();

    Console.WriteLine("Настройка кнопки Меню...");
    
    /*WebAppInfo wai = new WebAppInfo();
    wai.Url = "https://45.8.230.244:8080/";
    MenuButtonWebApp mbwa = new MenuButtonWebApp();
    mbwa.Text = "Starosta panel";
    mbwa.WebApp = wai;
    await botClient.SetChatMenuButtonAsync(null, mbwa);*/
    
    await botClient.SetChatMenuButtonAsync(null, new MenuButtonCommands());
    
    Console.WriteLine("Успешно!\nНастройка команд бота...");

    List<TelegramBotCommand> commands = new List<TelegramBotCommand>();
    commands.Add(new TelegramBotCommand(botClient, "me", "Краткая информация о студенте", async (_context) =>
    {
        try
        {
            await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                                              _context.User.ToString(),
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
                                        async (_context) =>
                                        {
                                            try
                                            {
                                                var passes = await HostApiAccess.GetPassesByStudentID(_context.User.ID, DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now));
                                                string answer = string.Join("\n\n", passes);

                                                if (string.IsNullOrWhiteSpace(answer)) answer = "Пропусков нет";
                                                
                                                await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
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
    
    commands.Add(new TelegramBotCommand(botClient, "passes", "Информация о пропусках за определенный день",
                                        async (_context) =>
                                        {
                                            try
                                            {
                                                DateTime date;
                                                if (!DateTime.TryParse(_context.Message.Text.Split(' ')[^1], out date))
                                                    return false;
                                                //var passes = await HostApiAccess.GetPasses(_context.User.ID, date);
                                                //string answer = string.Join("\n\n", passes.Select(x => x.ToString()));

                                                string answer = "";
                                                
                                                if (string.IsNullOrWhiteSpace(answer)) answer = "Пропусков нет";
                                                
                                                var result = await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
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
        AllowedUpdates = new [] { UpdateType.Message, UpdateType.CallbackQuery },
        ThrowPendingUpdates = true,
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
        Message updateMessage;
        switch (update.Type)
        {
            case UpdateType.Message:
                updateMessage = update.Message;
                break;
            case UpdateType.CallbackQuery:
                updateMessage = update.CallbackQuery.Message;
                break;
            default:
                return;
        }

        switch (updateMessage.Type)
        {
            case MessageType.Text:
                ProcessTextMessage(botClient, updateMessage, cancellationToken);
                return;
            case MessageType.Document:
            case MessageType.Photo:
                ProcessImageMessage(botClient, updateMessage, cancellationToken);
                return;
            default:
                return;
        }

        //TODO: получение студента по id телеграмм
        //TODO: если студент не найден и если сообщение содержит что то кроме цифр, просьба ввести номер зачётной книжки
        //TODO: если студент не найден и сообщение содержит только цифры, поиск студента с указанной зачёткой
        //TODO: если студент с номером зачётной книжки найден, проверка поля tgId
        //TODO: если tgId пуст, сохранение текущего tgId с сообщением "успешно авторизовано"
        //TODO: если tgId занят, сообщение о том, что аккаунт занят
        //TODO: если студент найден, поиск и исполнение введённой команды
    }

    async Task ProcessTextMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var student = await HostApiAccess.GetStudentByTgID(message.Chat.Id);
        if (student.ID == -1)
        {
            string[] messageTextSplitted = message.Text.Split(' ');
            if (messageTextSplitted.Length != 2)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для инструкций обратитесь к вашему старосте.");
                return;
            }

            if(await HostApiAccess.SetStudentTgID(messageTextSplitted[0], messageTextSplitted[1], message.Chat.Id))
                await botClient.SendTextMessageAsync(message.Chat.Id, "Регистрация прошла успешно.");
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для инструкций обратитесь к вашему старосте.");
            return;
        }

        if (message.Text.First() != '/') return;
        
        var messageFullCommand = string.Join("", message.Text.Skip(1)).Split(' ');
        if (messageFullCommand.Length == 0) return;

        commands.FirstOrDefault(x => x.Command.Command == messageFullCommand[0])?.Execute(new CommandContext()
        {
            Client = botClient,
            Command = messageFullCommand[0],
            Message = message,
            Parameters = messageFullCommand.Length == 1 ? new[] { "" } : messageFullCommand.Skip(1).ToArray(),
            User = student,
        });
    }

    async Task ProcessImageMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var student = await HostApiAccess.GetStudentByTgID(message.Chat.Id);
        if (student.ID == -1)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Для инструкций обратитесь к вашему старосте.");
            return;
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
    Console.WriteLine("Если ошибка появляется опять, повторите попытку позже.");
    return;
}
