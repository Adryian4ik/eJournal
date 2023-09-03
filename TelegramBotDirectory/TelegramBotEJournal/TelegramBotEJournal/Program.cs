using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotEJournal;
using TelegramBotEJournal.Commands;
using TelegramBotEJournal.Entities;

try
{
    Dictionary<long, DateOnly> userdates = new();
    
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
                                                if (!DateOnly.TryParse(_context.Parameters.FirstOrDefault(),
                                                        out var date)) return false;

                                                var passes = await HostApiAccess.GetPassesByStudentID(_context.User.ID, date, date);

                                                string result = string.Join(" ", passes.Select(x => x.ToString()));
                                                if (string.IsNullOrWhiteSpace(result)) result = "Пропусков нет";

                                                await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                                                    result,
                                                    cancellationToken: cts.Token);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                                return false;
                                            }

                                            return true;
                                        }));
    
    commands.Add(new TelegramBotCommand(botClient, "schedule", "Информация о расписании за определенный день",
        async (_context) =>
        {
            try
            {
                if (!DateOnly.TryParse(_context.Parameters.FirstOrDefault(),
                        out var date)) return false;

                var schedule = await HostApiAccess.GetStudentSchedule(_context.User.GroupID);

                /*string result = string.Join("\n",
                    schedule.Select(x =>
                        $"{x.Key}\n{string.Join("\n", x.Value.Select(x =>
                            $"{x.Key}\n{string.Join("\n", x.Value.Select(x =>
                                string.Join("\n", x.Select(x => x.ToString()))))}"))
                        }\n\n")
                    );*/

                var startDate = new DateOnly(date.Month < 9 ? date.Year - 1 : date.Year, 9, 1);
                var deltaTime = date.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
                var weekType =
                    (new DateOnly(new DateTime(deltaTime.Ticks).Year, new DateTime(deltaTime.Ticks).Month,
                        new DateTime(deltaTime.Ticks).Day).DayNumber / 7) % 2 == 0
                        ? "Верхняя"
                        : "Нижняя";
                /*string result =
                    $"Неделя - {weekType}\n{string.Join("\n", schedule[weekType].Select(x =>
                        $"{x.Key}\n{string.Join("\n", x.Value.Select(x =>
                            string.Join("\n", x.Select(x => x.ToString()))))}"))}";*/

                string dayOfWeek = date.DayOfWeek switch
                {
                    DayOfWeek.Friday => "Пятница",
                    DayOfWeek.Monday => "Понедельник",
                    DayOfWeek.Saturday => "Суббота",
                    DayOfWeek.Sunday => "Воскресение",
                    DayOfWeek.Thursday => "Пятница",
                    DayOfWeek.Tuesday => "Вторник",
                    DayOfWeek.Wednesday => "Среда",
                };

                string lessons = string.Join("\n",
                    schedule[weekType][dayOfWeek].Select(x => string.Join(" ", x.Select(x => x.ToString()))));
                
                string result =
                    $"Неделя - {weekType}\n{dayOfWeek}\n{(string.IsNullOrWhiteSpace(lessons) ? "Пар нет" : lessons)}";
                
                if (string.IsNullOrWhiteSpace(result)) result = "Расписания нету";

                await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                    result,
                    cancellationToken: cts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }));
    
    commands.Add(new TelegramBotCommand(botClient, "currentmonthkeyboard", "Перенести клавиатуру на текущий месяц", async (_context) =>
    {
        try
        {
            DateOnly currentUserDate;
            if (!userdates.ContainsKey(_context.User.ID)) userdates.Add(_context.User.ID, DateOnly.FromDateTime(DateTime.Now));
            userdates[_context.User.ID] = DateOnly.FromDateTime(DateTime.Now);
            currentUserDate = userdates[_context.User.ID];
            
            await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                $"Клавиатура обновлена.\nВыбранный месяц: {currentUserDate.ToString("MM.yyyy")}",
                replyMarkup: await GenerateReplyKeyboardByDate(currentUserDate),
                cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }));
    
    commands.Add(new TelegramBotCommand(botClient, "updatekeyboard", "Обновить клавиатуру дат", async (_context) =>
    {
        try
        {
            DateOnly currentUserDate;
            if (!userdates.ContainsKey(_context.User.ID)) userdates.Add(_context.User.ID, DateOnly.FromDateTime(DateTime.Now));
            
            currentUserDate = userdates[_context.User.ID];
            
            await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                $"Клавиатура обновлена.\nВыбранный месяц: {currentUserDate.ToString("MM.yyyy")}",
                replyMarkup: await GenerateReplyKeyboardByDate(currentUserDate),
                cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }));
    
    commands.Add(new TelegramBotCommand(botClient, "prevmonthkeyboard", "Предыдущий месяц", async (_context) =>
    {
        try
        {
            DateOnly currentUserDate;
            if (!userdates.ContainsKey(_context.User.ID)) userdates.Add(_context.User.ID, DateOnly.FromDateTime(DateTime.Now));
            userdates[_context.User.ID] = userdates[_context.User.ID].AddMonths(-1);
            currentUserDate = userdates[_context.User.ID];
            
            await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                $"Выбранный месяц: {currentUserDate.ToString("MM.yyyy")}", replyMarkup: await GenerateReplyKeyboardByDate(currentUserDate),
                cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }));
    
    commands.Add(new TelegramBotCommand(botClient, "nextmonthkeyboard", "Следующий месяц", async (_context) =>
    {
        try
        {
            DateOnly currentUserDate;
            if (!userdates.ContainsKey(_context.User.ID)) userdates.Add(_context.User.ID, DateOnly.FromDateTime(DateTime.Now));
            userdates[_context.User.ID] = userdates[_context.User.ID].AddMonths(1);
            currentUserDate = userdates[_context.User.ID];
            
            await _context.Client.SendTextMessageAsync(_context.Message.Chat.Id,
                $"Выбранный месяц: {currentUserDate.ToString("MM.yyyy")}", replyMarkup: await GenerateReplyKeyboardByDate(currentUserDate),
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
        await Task.Delay(new TimeSpan(3, 0, 0));
        userdates.Clear();
        GC.Collect();
    }

    // Send cancellation request to stop bot
    cts.Cancel();

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Message updateMessage;
        switch (update.Type)
        {
            case UpdateType.Message:
                updateMessage = update.Message!;
                break;
            case UpdateType.CallbackQuery:
                updateMessage = update.CallbackQuery.Message;
                updateMessage.Text = update.CallbackQuery.Data;
                break;
            default:
                return;
        }
        
        Console.WriteLine(updateMessage.From.Username + " " + updateMessage.Text);

        switch (updateMessage.Type)
        {
            case MessageType.Text:
                await ProcessTextMessage(botClient, updateMessage, cancellationToken);
                return;
            case MessageType.Document:
            case MessageType.Photo:
                await ProcessImageMessage(botClient, updateMessage, cancellationToken);
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

            if (await HostApiAccess.SetStudentTgID(messageTextSplitted[0], messageTextSplitted[1], message.Chat.Id))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Регистрация прошла успешно.");
                var context = new CommandContext();
                context.User = student;
                context.Client = botClient;
                context.Message = message;
                commands.FirstOrDefault(x => x.Command.Command == "currentmonthkeyboard")?.Execute(context);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для инструкций обратитесь к вашему старосте.");
            return;
        }

        if(message.Text[0] == '/')
        {
            await ProcessBotCommand(botClient, message, student, cancellationToken);
        }
        else
        {
            var messageDate = message.Text.Split(' ').FirstOrDefault() ?? "";
            if (string.IsNullOrWhiteSpace(messageDate)) return;

            if (!DateOnly.TryParse(messageDate, out DateOnly date))
            {
                CommandContext context;
                switch (message.Text)
                {
                    case "Назад":
                        context = new CommandContext();
                        context.User = student;
                        context.Client = botClient;
                        context.Message = message;
                        commands.FirstOrDefault(x => x.Command.Command == "prevmonthkeyboard")?.Execute(context);
                        return;
                    case "Обновить":
                        context = new CommandContext();
                        context.User = student;
                        context.Client = botClient;
                        context.Message = message;
                        commands.FirstOrDefault(x => x.Command.Command == "updatekeyboard")?.Execute(context);
                        return;
                    case "Вперёд":
                        context = new CommandContext();
                        context.User = student;
                        context.Client = botClient;
                        context.Message = message;
                        commands.FirstOrDefault(x => x.Command.Command == "nextmonthkeyboard")?.Execute(context);
                        return;
                    case "Сейчас":
                        context = new CommandContext();
                        context.User = student;
                        context.Client = botClient;
                        context.Message = message;
                        commands.FirstOrDefault(x => x.Command.Command == "currentmonthkeyboard")?.Execute(context);
                        return;
                    default:
                        if (!int.TryParse(message.Text, out int day)) return;
                        if (!userdates.ContainsKey(student.ID))
                        {
                            context = new CommandContext();
                            context.User = student;
                            context.Client = botClient;
                            context.Message = message;
                            commands.FirstOrDefault(x => x.Command.Command == "currentmonthkeyboard")?.Execute(context);
                        }

                        date = new DateOnly(userdates[student.ID].Year, userdates[student.ID].Month, day);

                        break;
                }
            }

            InlineKeyboardButton[] ikb = new[]
            {
                InlineKeyboardButton.WithCallbackData("Расписание дня", $"/schedule {date.ToString()}"),
                InlineKeyboardButton.WithCallbackData("Пропуски за день", $"/passes {date.ToString()}"),
            };
            InlineKeyboardMarkup keymark = new InlineKeyboardMarkup(ikb);
            
            await botClient.SendTextMessageAsync(message.Chat.Id, date.ToString("dd.MM.yyyy"), 
                replyMarkup: keymark,
                cancellationToken: cancellationToken);
        }
    }

    async Task ProcessBotCommand(ITelegramBotClient botClient, Message message, Student student, CancellationToken cancellationToken)
    {
        if (message.Text.First() != '/') return;
        
        var messageFullCommand = string.Join("", message.Text.Skip(1)).Split(' ');
        if (messageFullCommand.Length == 0) return;

        await (commands.FirstOrDefault(x => x.Command.Command == messageFullCommand[0])?.Execute(new CommandContext()
        {
            Client = botClient,
            Command = messageFullCommand[0],
            Message = message,
            Parameters = messageFullCommand.Length == 1 ? new[] { "" } : messageFullCommand.Skip(1).ToArray(),
            User = student,
        }) ?? Task.CompletedTask);
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

    async Task<ReplyKeyboardMarkup> GenerateReplyKeyboardByDate(DateOnly date)
    {
        var buttPrev = new KeyboardButton("Назад");
        var buttUpdate = new KeyboardButton("Обновить");
        var buttNext = new KeyboardButton("Вперёд");
        var buttCurr = new KeyboardButton("Сейчас");

        List<KeyboardButton> enumKey = new List<KeyboardButton>(35);

        var firstDay = new DateOnly(date.Year, date.Month, 1);
        switch (firstDay.DayOfWeek)
        {
            case (DayOfWeek)0:
                enumKey.AddRange(Enumerable.Repeat(new KeyboardButton("\n"), 6));
                break;
            default:
                enumKey.AddRange(Enumerable.Repeat(new KeyboardButton("\n"), (byte)firstDay.DayOfWeek - 1));
                break;
        }
        
        enumKey.AddRange(Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))
            .Select(x => new DateOnly(date.Year, date.Month, x))
            .Select(x => new KeyboardButton(x.ToString("dd"))));
        
        var lastDay = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        switch (lastDay.DayOfWeek)
        {
            case (DayOfWeek)0:
                break;
            default:
                enumKey.AddRange(Enumerable.Repeat(new KeyboardButton("\n"), 7 - (byte)lastDay.DayOfWeek));
                break;
        }

        return new ReplyKeyboardMarkup(enumKey.Chunk(7).Append(new[] { buttPrev, buttUpdate, buttNext })
            .Append(new[] { buttCurr }));

        /*return new ReplyKeyboardMarkup(Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))
            .Select(x => new DateOnly(date.Year, date.Month, x))
            .Select(x => new KeyboardButton(x.ToString("dd"))).Prepend(buttNext).Prepend(buttUpdate)
            .Prepend(buttPrev).Chunk(7));*/
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
