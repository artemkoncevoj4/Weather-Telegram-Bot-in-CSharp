using System.Collections.Concurrent;
using System.Text;
using DotNetEnv;
using GlobalHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    internal class Program
    {
        internal static TelegramBotClient _botClient;

        public static readonly string botToken = Env.GetString("BOT_TOKEN");
        public static readonly string Weather_api_key = Env.GetString("OPEN_WEATHER_API");
        public static Database.WeatherDB WeatherDB = new();

        public enum UserState
        {
            MainMenu,
            WeatherMenu,
            WaitingForCity,
        }

        public static readonly ConcurrentDictionary<long, UserState> UserStates = new ConcurrentDictionary<long, UserState>();
        internal static readonly ConcurrentDictionary<long, string> TempData = new ConcurrentDictionary<long, string>();
       

        //* Инициализация фабрики кнопки "Назад"
        public static class BackButtonHandlers
        {
            public static BackButtonFactory Factory { get; } = new BackButtonFactory();

            static BackButtonHandlers()
            {
                Factory.RegisterHandler(UserState.WaitingForCity,
                    (bot, msg, token) => new WeatherMenu.Handler.InsertCityBackButton(bot, msg, token));
                    
                 Factory.RegisterHandler(UserState.WeatherMenu,
                    (bot, msg, token) => new DefaultBackButton(bot, msg, token));
                
                // Здесь можно зарегистрировать другие обработчики
                // Factory.RegisterHandler(Program.UserState.OtherState, ...);
            }
        }

        static async Task Main(string[] args)
        {
            Env.Load();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                if (string.IsNullOrEmpty(botToken))
                    throw new ArgumentNullException("TELEGRAM_BOT_TOKEN не установлен");

                _botClient = new TelegramBotClient(botToken);
                
                await WeatherDB.InitializeAsync();

                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Бот остановлен");
                };

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };

                _botClient.StartReceiving(
                    UpdateHandler.HandleUpdateAsync,
                    UpdateHandler.HandleErrorAsync,
                    receiverOptions,
                    cts.Token
                    
                );

                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запуска бота: {ex.Message}");
            }
        }
    }
}