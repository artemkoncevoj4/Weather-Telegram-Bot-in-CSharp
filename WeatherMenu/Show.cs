using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;
namespace WeatherMenu
{
    public class Menu
    {
        public static async Task Show(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken)
        {
            Program.UserStates[chatId] = Program.UserState.WeatherMenu;

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("🏙️ Выбрать город") },
                new[] { new KeyboardButton("🌡️ Узнать сводку") },
                new[] { new KeyboardButton("◀️ Назад") }
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendMessage(
                chatId: chatId,
                text: "Выберите действие:",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}