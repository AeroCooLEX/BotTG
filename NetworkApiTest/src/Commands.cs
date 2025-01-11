using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.Linq;

namespace BotTG
{
    public class Commands : ModSystem
    {
        private const string BotToken = "YOUR_TOKEN"; // Замените на ваш токен
        private const string ChatId = "CHAT_ID"; // Замените на ID вашего канала
        private const string TelegramApiUrl = "https://api.telegram.org/bot";
        private HttpClient httpClient;

        // Ссылки на API сервера, которая будет доступна через ModSystem
        private ICoreServerAPI api;

        // Этот метод запускается при старте сервера
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            this.api = api; // Сохраняем ссылку на ICoreServerAPI

            // Инициализируем httpClient для взаимодействия с Telegram API
            httpClient = new HttpClient();
        }

        // Метод для обработки команд, поступающих от Telegram
        public async Task HandleCommand(string message)
        {
            if (string.IsNullOrEmpty(message) || !message.StartsWith("/")) return;

            // Отправляем команду в консоль перед обработкой
            InjectConsole($"Команда от Telegram: {message}");

            if (message.StartsWith("/stats"))
            {
                var stats = GetServerStats();
                await SendMessageToTelegram(stats);
                InjectConsole(stats); // Отправить в консоль
            }
            else if (message.StartsWith("/kick"))
            {
                var playerName = message.Split(' ').ElementAtOrDefault(1);
                if (playerName != null)
                {
                    Disconnect(playerName);  // Вызываем метод для кика
                    string kickMessage = $"Игрок {playerName} был кикнут.";
                    await SendMessageToTelegram(kickMessage);
                    InjectConsole(kickMessage); // Отправить в консоль
                }
            }
            else if (message.StartsWith("/stop"))
            {
                ShutDown();
                string stopMessage = "Сервер остановлен.";
                await SendMessageToTelegram(stopMessage);
                InjectConsole(stopMessage); // Отправить в консоль
            }
            else if (message.StartsWith("/annonce"))
            {
                var annonceMessage = message.Substring("/annonce".Length).Trim();
                if (!string.IsNullOrEmpty(annonceMessage))
                {
                    BroadcastMessage(annonceMessage);
                    string broadcastMessage = $"Сообщение отправлено всем игрокам: {annonceMessage}";
                    await SendMessageToTelegram(broadcastMessage);
                    InjectConsole(broadcastMessage); // Отправить в консоль
                }
            }
            else
            {
                string unknownCommand = "Неизвестная команда.";
                await SendMessageToTelegram(unknownCommand);
                InjectConsole(unknownCommand); // Отправить в консоль
            }
        }

        // Метод для получения статистики о сервере
        private string GetServerStats()
        {
            // Здесь можно заменить на реальные данные
            var stats = "Статистика сервера:\n- Игроков онлайн: 5\n- Память: 50MB\n- Процессор: 20%";
            return stats;
        }

        // Метод для кика игрока
        private void Disconnect(string playerName)
        {
            var player = api.World.AllPlayers.FirstOrDefault(p => p.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)); // Поиск игрока по имени
            if (player != null)
            {
                // Приводим IPlayer к IServerPlayer для использования метода Kick
                var serverPlayer = player as IServerPlayer;
                serverPlayer?.Disconnect("Вы были кикнуты через Telegram-бот.");
            }
            else
            {
                string errorMessage = $"Игрок с именем {playerName} не найден.";
                InjectConsole(errorMessage); // Отправить в консоль
                Console.WriteLine(errorMessage);
            }
        }

        // Метод для остановки сервера
        private void ShutDown()
        {
            // Используем метод StopServer из ICoreServerAPI
            api.Server.ShutDown();
        }

        // Метод для отправки широковещательного сообщения
        private void BroadcastMessage(string message)
        {
            // Указываем тип чата при отправке сообщения
            api.BroadcastMessageToAllGroups(message, EnumChatType.CommandSuccess, null);
        }

        // Метод для отправки сообщения в Telegram
        private async Task SendMessageToTelegram(string message)
        {
            try
            {
                string url = $"{TelegramApiUrl}{BotToken}/sendMessage";

                var content = new StringContent(
                    $"{{\"chat_id\":\"{ChatId}\",\"text\":\"{message}\"}}",
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ошибка отправки сообщения: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при отправке сообщения: {ex.Message}");
            }
        }

        // Метод для отправки сообщения в консоль сервера через InjectConsole
        private void InjectConsole(string message)
        {
            api.Logger.Notification(message); // Отправляет сообщение в консоль сервера
        }

        public override void Dispose()
        {
            httpClient?.Dispose();
            base.Dispose();
        }
    }
}
