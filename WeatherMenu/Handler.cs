using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using TelegramBot; //* namespace Program.cs
namespace WeatherMenu
{
    public static class Handler
    {
        public static async Task HandleWeatherMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;
            
            if (message.Text.Equals("🏙️ Выбрать город", StringComparison.OrdinalIgnoreCase))
            {
                Program.UserStates[chatId] = Program.UserState.WaitingForCity;

                var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("◀️ Назад") })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Введите название города:",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            else if (message.Text.Equals("🌡️ Узнать сводку", StringComparison.OrdinalIgnoreCase))
            {
                string city;
                Database.WeatherDB weatherDB = new();

                string firstname = message.From.FirstName ?? "";
                string lastname = message.From.LastName ?? "";
                string nickname = message.From.Username ?? "";

                city = await weatherDB.GetCity(firstname, lastname, nickname);
                

                if (!string.IsNullOrEmpty(city))
                {
                    await GetAndSendWeather(botClient, chatId, city, message.From, cancellationToken);
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ Для вас нет сохраненного города. Пожалуйста, сначала выберите город.",
                        cancellationToken: cancellationToken);
                }
            }
        }

        public static async Task HandleCityInput(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {

            long chatId = message.Chat.Id;
            string city = message.Text;
            long userId = message.From.Id;
            string first_name = message.From.FirstName;
            string last_name = message.From.LastName;
            string nickName = message.From.Username;
            Console.WriteLine($"Setting city for UserId={userId}, City={city}");

            await Program.WeatherDB.AddOrUpdateUser(first_name, last_name, nickName, city);
            Program.UserStates[message.From.Id] = Program.UserState.WeatherMenu;


            //TODO Пересмотреть
            //if (!Regex.IsMatch(city, @"^[a-zA-Zа-яА-ЯёЁ\s\-\'\.]{2,50}$"))
            //{
            //    await botClient.SendMessage(
            //        chatId: chatId,
            //        text: "❌ Недопустимое название города. Используйте только буквы, пробелы и дефисы",
            //        cancellationToken: cancellationToken);
            //    return;
            //}

            Program.UserStates[chatId] = Program.UserState.WeatherMenu;
            await Menu.Show(botClient, chatId, cancellationToken);
            await GetAndSendWeather(botClient, chatId, city, message.From, cancellationToken);
        }

        public static async Task GetAndSendWeather(
            ITelegramBotClient botClient,
            long chatId,
            string city,
            User user,
            CancellationToken cancellationToken)
        {
            string apiKey = Program.Weather_api_key;
            if (string.IsNullOrEmpty(apiKey))
            {
                await botClient.SendMessage(chatId: chatId, text: "❌ API ключ не настроен", cancellationToken: cancellationToken);
                return;
            }

            var weatherService = new Service(apiKey);
            var weather = await weatherService.GetWeatherAsync(city);
            if (weather != null)
            {
                string userInfo = $"{user.FirstName} {user.LastName}";
                if (!string.IsNullOrEmpty(user.Username))
                    userInfo += $" (@{user.Username})";

                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Погода для {userInfo} в {weather.City}:\n" +
                          $"Температура: {weather.Main.Temperature}°C\n" +
                          $"Ощущается как: {weather.Main.FeelsLike}°C\n" +
                          $"Описание: {weather.Weather[0].Description}\n" +
                          $"Ветер: {weather.Wind.Speed} м/с",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Не удалось получить данные о погоде для города {city}",
                    cancellationToken: cancellationToken);
            }
        }
        internal class InsertCityBackButton : GlobalHandler.BackButton
        {
            public InsertCityBackButton(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
                : base(botClient, message, cancellationToken)
            {
                
            }

            public override async Task HandleBackButton()
            {
                await Menu.Show(botClient, message.Chat.Id, cancellationToken);
            }
        }
    }
}