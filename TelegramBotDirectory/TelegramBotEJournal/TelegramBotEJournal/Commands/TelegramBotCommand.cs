// /*
//     Copyright © 2023 Anderwafe. All rights reserved.
// 
//     Copyright 2023 Anderwafe
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing,
//     software distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and limitations under the License.
// */

using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotEJournal.Commands;

public class TelegramBotCommand
{
    private ITelegramBotClient client;
    private BotCommand _command;
    private Func<CommandContext, Task<bool>> _commandAction;
    
    public BotCommand Command
    {
        get => _command;
    }
    
    public TelegramBotCommand(ITelegramBotClient botClient, string command, string description, Func<CommandContext, Task<bool>> reaction)
    {
        client = botClient;
        _command = new BotCommand();
        _command.Command = (string)command.Clone();
        _command.Description = (string)description.Clone();
        _commandAction = (Func<CommandContext, Task<bool>>)reaction.Clone();
    }

    public async Task<bool> Execute(CommandContext context)
    {
        return await _commandAction.Invoke(context);
    }
}