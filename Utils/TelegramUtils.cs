using Telegram.Bot.Types;

namespace TelegramBot.Utils
{
    public static class TelegramUtils
    {
        public static string GetSenderInfo(User user)
        {
            if (user == null) return "Неизвестный отправитель";

            string username = string.IsNullOrEmpty(user.Username)
                ? "<без username>"
                : $"@{user.Username}";

            return $"{user.FirstName} {user.LastName}".Trim() + $" ({username})";
        }
    }
}