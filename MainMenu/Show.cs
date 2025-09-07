using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

using TelegramBot; //* namespace Program.cs

namespace MainMenu
{
    public static class Menu
    {
        public static async Task Show(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            Program.UserStates[chatId] = Program.UserState.MainMenu;
            Program.TempData.TryRemove(chatId, out _);

            var buttons = new[]
            {
                new[] { new KeyboardButton("🌦️ Узнать погоду") },
                new[] { new KeyboardButton("📅 Дата и Время") },
            };

            var keyboard = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };

            await botClient.SendMessage(
                chatId: chatId,
                text: "👑 Главное меню 👑",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}