using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBot.Utils;


using TelegramBot; //* namespace Program.cs

namespace GlobalHandler
{
    public static class UpdateHandler
    {
        public static async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is Message message && message.Text != null)
                {
                    long chatId = message.Chat.Id;
                    long userId = message.From.Id;

                    string senderInfo = TelegramUtils.GetSenderInfo(message.From);

                    Console.WriteLine($"Получено сообщение от {senderInfo}: {message.Text}");
                    if (message.ReplyToMessage != null)
                    {
                        if (Program.UserStates.TryGetValue(chatId, out var state) &&
                            state == Program.UserState.WaitingForCity)
                        {
                            await WeatherMenu.Handler.HandleCityInput(botClient, message, cancellationToken);
                            return;
                        }
                    }
                    
                    if (!Program.UserStates.ContainsKey(chatId))
                    {
                        Program.UserStates[chatId] = Program.UserState.MainMenu;
                    }

                    if (message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                    {
                        await MainMenu.Menu.Show(botClient, chatId, cancellationToken);
                    }
                    else if (message.Text.Equals("◀️ Назад", StringComparison.OrdinalIgnoreCase))
                    {
                        var backButton = Program.BackButtonHandlers.Factory.GetHandler(botClient, message, cancellationToken);
                        await backButton.HandleBackButton();

                    }
                    else
                    {
                        // Обработка по состоянию пользователя
                        switch (Program.UserStates[chatId])
                        {
                            case Program.UserState.MainMenu:
                                await MainMenu.Handler.HandleMainMenu(botClient, message, cancellationToken);
                                break;

                            case Program.UserState.WeatherMenu:
                                await WeatherMenu.Handler.HandleWeatherMenu(botClient, message, cancellationToken);
                                break;

                            case Program.UserState.WaitingForCity:
                                await WeatherMenu.Handler.HandleCityInput(botClient, message, cancellationToken);
                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Получаем chatId из сообщения
                long chatId = update.Message?.Chat.Id ?? 0;
                if (chatId != 0)
                {
                }
                Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
            }
        }

        public static Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(exception is ApiRequestException apiEx
                ? $"Ошибка Telegram API: {apiEx.ErrorCode} - {apiEx.Message}"
                : exception.ToString());

            return Task.CompletedTask;
        }

    }
    public class BackButtonFactory
    {
        private readonly Dictionary<Program.UserState, Func<ITelegramBotClient, Message, CancellationToken, BackButton>> _handlers
        = new Dictionary<Program.UserState, Func<ITelegramBotClient, Message, CancellationToken, BackButton>>();

        internal void RegisterHandler(Program.UserState state, Func<ITelegramBotClient, Message, CancellationToken, BackButton> handlerFactory)
        {
            _handlers[state] = handlerFactory;
        }
         public BackButton GetHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var currentState = Program.UserStates.GetValueOrDefault(message.Chat.Id);
            
            if (_handlers.TryGetValue(currentState, out var handlerFactory))
            {
                return handlerFactory(botClient, message, cancellationToken);
            }
            
            // Обработчик по умолчанию
            return new DefaultBackButton(botClient, message, cancellationToken);
        }
    }
    public abstract class BackButton
    {
        protected readonly ITelegramBotClient botClient;
        protected readonly Message message;
        protected readonly CancellationToken cancellationToken;

        protected BackButton(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            this.botClient = botClient;
            this.message = message;
            this.cancellationToken = cancellationToken;
        }

        public abstract Task HandleBackButton();
    }
    public class DefaultBackButton : BackButton
    {
        public DefaultBackButton(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) 
            : base(botClient, message, cancellationToken) { }

        public override async Task HandleBackButton()
        {
            await MainMenu.Menu.Show(botClient, message.Chat.Id, cancellationToken);
        }
    }
}
