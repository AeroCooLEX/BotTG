using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using System.Linq;

namespace BotTG
{
    public class TelegramMod : ModSystem
    {
        private const string BotToken = "YOUR_TOKEN";  // Замените на ваш токен
        private const string ChatId = "CHAT_ID";  // Замените на ID вашего канала
        private const string TelegramApiUrl = "https://api.telegram.org/bot";
        private HttpClient httpClient;

        // Добавляем поле для хранения ссылки на ICoreServerAPI
        private ICoreServerAPI api;

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            this.api = api; // Сохраняем ссылку на api

            httpClient = new HttpClient();

            // Подписка на события чата, входа и выхода игроков
            api.Event.PlayerChat += OnPlayerChat;
            api.Event.PlayerJoin += OnPlayerJoin;
            api.Event.PlayerDisconnect += OnPlayerLeave; // Подписка на событие ухода игрока
        }

        // Обработчик события чата
        private void OnPlayerChat(IServerPlayer player, int channelId, ref string message, ref string data, BoolRef consumed)
        {
            if (string.IsNullOrEmpty(message)) return;

            string text = $"Игрок {player.PlayerName} написал: {message}";
            SendMessageToTelegram(text).Wait();
        }

        // Обработчик события входа игрока
        private void OnPlayerJoin(IServerPlayer player)
        {
            // Собираем список онлайн игроков
            var onlinePlayers = string.Join(", ", api.World.AllPlayers.Select(p => p.PlayerName));
            string text = $"Игрок {player.PlayerName} присоединился к серверу.\n" +
                          $"Список игроков онлайн: {onlinePlayers}";

            // Отправляем сообщение в чат сервера
            api.BroadcastMessageToAllGroups(text, EnumChatType.Notification, null);

            // Отправляем в Telegram
            SendMessageToTelegram(text).Wait();
        }

        // Обработчик события ухода игрока
        private void OnPlayerLeave(IServerPlayer player)
        {
            // Собираем список оставшихся игроков
            var remainingPlayers = string.Join(", ", api.World.AllPlayers.Select(p => p.PlayerName));
            string text = $"Игрок {player.PlayerName} покинул сервер.\n" +
                          $"Оставшиеся игроки онлайн: {remainingPlayers}";

            // Отправляем сообщение в чат сервера
            api.BroadcastMessageToAllGroups(text, EnumChatType.Notification, null);

            // Отправляем в Telegram
            SendMessageToTelegram(text).Wait();
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

        public override void Dispose()
        {
            httpClient?.Dispose();
            base.Dispose();
        }
    }
}
