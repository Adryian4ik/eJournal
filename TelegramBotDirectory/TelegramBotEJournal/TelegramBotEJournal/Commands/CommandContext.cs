using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotEJournal.Entities;

namespace TelegramBotEJournal.Commands;

public class CommandContext
{
    public ITelegramBotClient Client { get; set; }
    public Student User { get; set; }
    public Message Message { get; set; }
    public string Command { get; set; }
    public string[] Parameters { get; set; }
}