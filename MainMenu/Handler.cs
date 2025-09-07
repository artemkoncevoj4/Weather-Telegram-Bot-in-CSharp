using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using TelegramBot;//* namespace Program.cs

namespace MainMenu
{
    public static class Handler
    {
        public static async Task HandleMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;

            if (message.Text.Equals("🌦️ Узнать погоду", StringComparison.OrdinalIgnoreCase))
            {
                await WeatherMenu.Menu.Show(botClient, chatId, cancellationToken);
            }
            else if (message.Text.Equals("📅 Дата и Время", StringComparison.OrdinalIgnoreCase))
            {
                DateTime now = DateTime.Now;
                string[] monthNames = {
                    "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
                };
                string[] dayNames = {
                    "Воскресенье", "Понедельник", "Вторник", "Среда",
                    "Четверг", "Пятница", "Суббота"
                };

                // Формируем календарь текущего месяца
                var calendar = GenerateCalendar(now.Year, now.Month, now.Day);

                await botClient.SendMessage(
                chatId: chatId,
                text: $"🕒 *Текущее время:* {now:HH:mm:ss}\n" +
                      $"📅 *Дата:* {EscapeMarkdown(now.ToString("dd.MM.yyyy"))}\n" + // Экранированы точки
                      $"🗓️ *День недели:* {dayNames[(int)now.DayOfWeek]}\n" +
                      $"📆 *Календарь на {monthNames[now.Month - 1]} {now.Year}:*\n" +
                      $"\n```\n{calendar}\n```",
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Неизвестная команда. Попробуйте вернуть бота в главное меню!",
                    cancellationToken: cancellationToken);
            }
        }
        private static string EscapeMarkdown(string text)
        {
            return text.Replace(".", "\\.")
                       .Replace("-", "\\-")
                       .Replace("(", "\\(")
                       .Replace(")", "\\)")
                       .Replace("!", "\\!")
                       .Replace("=", "\\=");
        }
        private static string GenerateCalendar(int year, int month, int currentDay)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Заголовок с днями недели
            string calendar = "Пн Вт Ср Чт Пт Сб Вс\n";

            // Отступ для первого дня
            int startDay = (int)firstDay.DayOfWeek;
            startDay = startDay == 0 ? 6 : startDay - 1; // Корректировка для понедельника

            // Добавляем пробелы перед первым днем
            calendar += new string(' ', startDay * 3);

            // Добавляем дни месяца
            for (int day = 1; day <= daysInMonth; day++)
            {
                // Выделяем текущий день
                string dayStr = day == currentDay ? $"[{day,2}]" : $"{day,2}";

                calendar += dayStr;

                // Перенос строки после воскресенья
                if ((startDay + day) % 7 == 0 && day != daysInMonth)
                    calendar += "\n";
                else
                    calendar += " ";
            }

            return calendar;
        }
    }
}