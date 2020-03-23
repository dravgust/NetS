using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetS.Core;
using NetS.Core.Utilities;
using NetS.Robot.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace NetS.Robot
{
    public class TelegramBotClientManager
    {
        /// <summary>Global application life cycle control - triggers when application shuts down.</summary>
        private readonly IApplicationLifeTime _appLifetime;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        private TelegramBotClient _client;
        private readonly IBotController _botController;

        public TelegramBotOptions Options;

        public TelegramBotClientManager(IOptions<TelegramBotOptions> options, ILoggerFactory loggerFactory, IBotController botController, IApplicationLifeTime appLifetime)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);

            this._appLifetime = Guard.NotNull(appLifetime, nameof(appLifetime));
            this._botController = Guard.NotNull(botController, nameof(botController));

            Guard.NotNull(options, nameof(options));
            this.Options = options.Value;
        }

        public void Start()
        {
            if (_client == null)
            {
                Guard.NotEmpty(Options.Key, nameof(Options.Key));

#if USE_PROXY
                var proxy = new WebProxy(new Uri(Options.Proxy, UriKind.Absolute)) { UseDefaultCredentials = true };
                _client = new TelegramBotClient(Options.Key, webProxy: proxy);
#else
                _client = new TelegramBotClient(Options.Key);
                #endif
                _client.OnMessage += (sender, messageEventArgs) =>
                    _botController.HandleAsync(messageEventArgs.Message, _client);

                _client.OnMessageEdited += (sender, messageEventArgs) =>
                    _botController.HandleAsync(messageEventArgs.Message, _client);

                _client.OnCallbackQuery += BotOnCallbackQueryReceived;
                _client.OnInlineQuery += BotOnInlineQueryReceived;
                _client.OnInlineResultChosen += BotOnChosenInlineResultReceived;
                //// UpdateType.Unknown:
                //// UpdateType.ChannelPost:
                //// UpdateType.EditedChannelPost:
                //// UpdateType.ShippingQuery:
                //// UpdateType.PreCheckoutQuery:
                //// UpdateType.Poll:
                _client.OnReceiveError += HandleErrorAsync;
                _client.OnReceiveGeneralError += HandleGeneralErrorAsync;
            }

            this._logger.LogInformation("Start Receiving...");
            _client.StartReceiving(Array.Empty<UpdateType>(), _appLifetime.ApplicationStopping);
        }

        public void Stop()
        {
            this._logger.LogInformation("Stop Receiving...");
            _client?.StopReceiving();
        }

        //private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        //{
        //    try
        //    {
        //        var message = messageEventArgs.Message;

        //        Console.WriteLine($"Receive message: {message.ToJSON()}");

        //        BotController.HandleAsync(message, _client);

        //        return;
        //        if (message.Type != MessageType.Text)
        //            return;

        //        var args = message.Text.Split(' ');
        //        switch (args.First())
        //        {
        //            case "/start":

        //                break;
        //            case "/help":

        //                break;
        //            case "/settings ":

        //                break;
        //            case "/bitcoin":
        //                await _client.SendTextMessageAsync(
        //                    chatId: message.Chat.Id,
        //                    text: $"{"..."}",
        //                    replyMarkup: new ReplyKeyboardRemove());

        //                break;
  
        //            // send inline keyboard
        //            //case "/inline":
        //            //    await _client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        //            //    // simulate longer running task
        //            //    await Task.Delay(500);

        //            //    var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //            //    {
        //            //    // first row
        //            //    new []
        //            //    {
        //            //        InlineKeyboardButton.WithCallbackData("1.1", "11"),
        //            //        InlineKeyboardButton.WithCallbackData("1.2", "12"),
        //            //    },
        //            //    // second row
        //            //    new []
        //            //    {
        //            //        InlineKeyboardButton.WithCallbackData("2.1", "21"),
        //            //        InlineKeyboardButton.WithCallbackData("2.2", "22"),
        //            //    }
        //            //});
        //            //    await _client.SendTextMessageAsync(
        //            //        chatId: message.Chat.Id,
        //            //        text: "Choose",
        //            //        replyMarkup: inlineKeyboard
        //            //    );
        //            //    break;

        //            // send custom keyboard
        //            //case "/keyboard":
        //            //    ReplyKeyboardMarkup ReplyKeyboard = new[]
        //            //    {
        //            //    new[] { "1.1", "1.2", "1.3" },
        //            //    new[] { "2.1", "2.2", "2.3" },
        //            //};
        //            //    await _client.SendTextMessageAsync(
        //            //        chatId: message.Chat.Id,
        //            //        text: "Choose",
        //            //        replyMarkup: ReplyKeyboard
        //            //    );
        //            //    break;

        //            // send a photo
        //            //case "/photo":
        //            //    await _client.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

        //            //    const string file = @"Files/tux.png";
        //            //    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        //            //    {
        //            //        var fileName = file.Split(Path.DirectorySeparatorChar).Last();
        //            //        await _client.SendPhotoAsync(
        //            //            chatId: message.Chat.Id,
        //            //            photo: new InputOnlineFile(fileStream, fileName),
        //            //            caption: "Nice Picture"
        //            //        );
        //            //    }
        //            //    break;

        //            //// request location or contact
        //            //case "/request":
        //            //    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
        //            //    {
        //            //    KeyboardButton.WithRequestLocation("Location"),
        //            //    KeyboardButton.WithRequestContact("Contact"),
        //            //});
        //            //    await _client.SendTextMessageAsync(
        //            //        message.Chat.Id,
        //            //        "Who or Where are you?",
        //            //        replyMarkup: RequestReplyKeyboard
        //            //    );
        //            //    break;
        //            case var c when !string.IsNullOrEmpty(c) && !c.StartsWith('/'):

        //                static async Task<(double price, double volume, double priceChange, double priceChangePerc)> GetLastTickAsync()
        //                {
        //                    var exchange = new CexIoService();
        //                    var data = await exchange.GetTickerAsync<dynamic>();
        //                    double.TryParse((string)data?.last, out var price);
        //                    double.TryParse((string)data?.volume, out var volume);
        //                    double.TryParse((string)data?.priceChange, out var priceChange);
        //                    double.TryParse((string)data?.priceChangePercentage, out var priceChangePerc);
        //                    return (price, volume, priceChange, priceChangePerc);
        //                }

        //                string result = null;
        //                if (long.TryParse(c, out var l) && args.Length > 1 && args[1].Equals("sat", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    var lastTick = await GetLastTickAsync();
        //                    result = ((lastTick.price / 100000000) * l).ToString("C8", CultureInfo.GetCultureInfo("en-US")).TrimEnd('0');
        //                }
        //                else if (double.TryParse(c, out var d))
        //                {
        //                    var lastTick = await GetLastTickAsync();
        //                    result = (lastTick.price * d).ToString("C", CultureInfo.GetCultureInfo("en-US"));
        //                }
        //                else
        //                {
        //                    var lastTick = await GetLastTickAsync();
        //                    var sb = new StringBuilder();
        //                    sb.AppendLine($"BTC/USD: <b>{lastTick.price} USD</b>");
        //                    sb.AppendLine($"Изменение: <b>{lastTick.priceChange} USD</b> ({lastTick.priceChangePerc}%)");
        //                    sb.AppendLine($"24ч объем: <b>{lastTick.volume:F2} BTC</b>");

        //                    result = sb.ToString();
        //                }

        //                await _client.SendTextMessageAsync(message.Chat.Id, result, replyToMessageId: message.MessageId, parseMode:ParseMode.Html);
        //                break;
        //            default:
        //                //const string usage = "Usage:\n" +
        //                //    "/inline   - send inline keyboard\n" +
        //                //    "/keyboard - send custom keyboard\n" +
        //                //    "/photo    - send a photo\n" +
        //                //    "/request  - request location or contact";
        //                //await _client.SendTextMessageAsync(
        //                //    chatId: message.Chat.Id,
        //                //    text: usage,
        //                //    replyMarkup: new ReplyKeyboardRemove()
        //                //);

        //                break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
                
        //    }
        //}

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            var message = callbackQuery.Message;

            if (callbackQuery.Data == "22")
            {
                await _client.AnswerCallbackQueryAsync(callbackQuery.Id, $"test");
            }
            else
            {
                await _client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{callbackQuery.Data}");
                await _client.AnswerCallbackQueryAsync(callbackQuery.Id);
            }

        }


        private async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            var inlineQuery = inlineQueryEventArgs.InlineQuery;

            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await _client.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private static async void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            await Task.Yield();
            var chosenInlineResult = chosenInlineResultEventArgs.ChosenInlineResult;
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        }

        public void HandleErrorAsync(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            var exception = receiveErrorEventArgs.ApiRequestException;
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            this._logger.LogError("Exception occurred: {0}", errorMessage.ToString());
        }

        public void HandleGeneralErrorAsync(object sender, ReceiveGeneralErrorEventArgs receiveGeneralErrorEventArgs)
        {
            var exception = receiveGeneralErrorEventArgs.Exception;
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            this._logger.LogError("Exception occurred: {0}", errorMessage.ToString());
        }
    }
}
