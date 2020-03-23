using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetS.Core.Utilities;
using NetS.Core.Utilities.Json;
using NetS.Robot.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NetS.Robot
{
    //{"message_id":106,"from":{"id":807177783,"is_bot":false,"first_name":"Антон","username":"dravgust","language_code":"ru"},"date":1583239445,"chat":{"id":-402711637,"type":"group","title":"Test","all_members_are_administrators":true},"edit_date":1583239712,"text":"/bitcoin btc","entities":[{"type":"bot_command","offset":0,"length":8}]}
    //{"message_id":96,"from":{"id":807177783,"is_bot":false,"first_name":"Антон","username":"dravgust","language_code":"ru"},"date":1583228833,"chat":{"id":-402711637,"type":"group","title":"Test","all_members_are_administrators":true},"reply_to_message":{"message_id":58,"from":{"id":807177783,"is_bot":false,"first_name":"Антон","username":"dravgust","language_code":"ru"},"date":1583177356,"chat":{"id":-402711637,"type":"group","title":"Test","all_members_are_administrators":true},"text":"/bitcoin","entities":[{"type":"bot_command","offset":0,"length":8}]},"text":"/bitcoin","entities":[{"type":"bot_command","offset":0,"length":8}]}
    //{
    //    "message_id": 51,
    //    "from": {
    //        "id": 807177783,
    //        "is_bot": false,
    //        "first_name": "Антон",
    //        "username": "dravgust",
    //        "language_code": "en"

    //    },
    //    "date": 1583174763,
    //    "chat": {
    //        "id": 807177783,
    //        "type": "private",
    //        "username": "dravgust",
    //        "first_name": "Антон"

    //    },
    //    "text": "/bitcoin",
    //    "entities": [{
    //        "type": "bot_command",
    //        "offset": 0,
    //        "length": 8

    //    }
    //    ]
    //}
    public interface IBotController
    {
        void HandleAsync(Message message, TelegramBotClient botClient);

        Task HandleExceptionAsync(Exception exception);
    }

    public class BotController : IBotController
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        private readonly Dictionary<string, Command> _commandsList = new Dictionary<string, Command>();

        private readonly MemoryCache _cache;

        public BotController(ILoggerFactory loggerFactory, List<Command> commands)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);

            foreach (var command in Guard.NotNull(commands, nameof(commands)))
            {
                _commandsList.Add(command.Name, command);
            }

            this._cache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = new TimeSpan(0, 1, 0) });
        }

        public async void HandleAsync(Message message, TelegramBotClient botClient)
        {
            this._logger.LogDebug($"Receive message: {message.ToJSON()}");

            if (message.Type != MessageType.Text) return;

            if(_commandsList.TryGetValue(message.Text.Split(' ').First(), out var command)
            || _commandsList.TryGetValue("/", out command))
            {
                try
                {
                    await command.Execute(message, botClient);
                }
                catch (Exception e)
                {
                    await HandleExceptionAsync(e);
                }
            }
        }
        public Task HandleExceptionAsync(Exception exception)
        {
            this._logger.LogError("Exception occurred: {0}", exception.ToString());
            return Task.CompletedTask;
        }
    }
}
