using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NetS.Robot.Commands
{
    public class DefaultCommand : Command
    {
        public override string Name => @"/";

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var args = message.Text.Split(' ');
            var c = args.First();

            static async Task<(double price, double volume, double priceChange, double priceChangePerc)> GetLastTickAsync()
            {
                var exchange = new CexIoService();
                var data = await exchange.GetTickerAsync<dynamic>();
                double.TryParse((string)data?.last, out var price);
                double.TryParse((string)data?.volume, out var volume);
                double.TryParse((string)data?.priceChange, out var priceChange);
                double.TryParse((string)data?.priceChangePercentage, out var priceChangePercentage);
                return (price, volume, priceChange, priceChangePercentage);
            }

            string result = null;
            if (long.TryParse(c, out var l) && args.Length > 1 && args[1].Equals("sat", StringComparison.InvariantCultureIgnoreCase))
            {
                var lastTick = await GetLastTickAsync();
                result = ((lastTick.price / 100000000) * l).ToString("C8", CultureInfo.GetCultureInfo("en-US")).TrimEnd('0');
            }
            else if (double.TryParse(c, out var d))
            {
                var lastTick = await GetLastTickAsync();
                result = (lastTick.price * d).ToString("C", CultureInfo.GetCultureInfo("en-US"));
            }
            else
            {
                var (price, volume, priceChange, priceChangePercentage) = await GetLastTickAsync();

                var sb = new StringBuilder();
                sb.AppendFormat("{0,-12} <b>{1} USD</b>", "BTC/USD:", price);
                sb.AppendLine();
                sb.AppendFormat("{0,-12} <b>{1} USD ({2}%)</b>", "Изменение:", priceChange, priceChangePercentage);
                sb.AppendLine();
                sb.AppendFormat("{0,-12} <b>{1:F2} BTC</b>", "24ч Объем:", volume);
                result = sb.ToString();
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, result, replyToMessageId: message.MessageId, parseMode: ParseMode.Html);
        }
    }
}
