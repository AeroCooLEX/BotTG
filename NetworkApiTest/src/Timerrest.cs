using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.Threading;

namespace BotTG
{
    public class ServerRestartMessageMod : ModSystem
    {
        private const string BotToken = "YOUR_TOKEN";  // Замените на ваш токен
        private const string ChatId = "CHAT_ID";  // Замените на ID вашего канала
        private const string TelegramApiUrl = "https://api.telegram.org/bot";
        private HttpClient httpClient;
        private Timer timer;

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            httpClient = new HttpClient();

            // Отправляем первое сообщение сразу после старта сервера
            string initialMessage = "Сервер был перезапущен. Статус онлайн! Следующий перезапуск через 3 часа 55 минут!";
            SendMessageToServer(api, initialMessage); // Отправить в чат сервера
            SendMessageToTelegram(initialMessage);   // Отправить в Telegram

            // Запускаем таймер для отправки сообщения через 3 часа 55 минут (235 минут)
            timer = new Timer(SendRestartMessage, api, TimeSpan.FromMinutes(235), TimeSpan.FromMinutes(235)); // Повторяем каждые 235 минут
        }

        // Метод для отправки сообщения в чат на сервере
        private void SendMessageToServer(ICoreServerAPI api, string message)
        {
            // Используем метод для отправки сообщений в чат, который не создает команду
            api.BroadcastMessageToAllGroups(message, EnumChatType.Notification, null); // Используем BroadcastMessage вместо SendGlobalMessage
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
                Console.WriteLine($"Ошибка при отправке сообщения в Telegram: {ex.Message}");
            }
        }

        // Метод, который будет вызываться через каждые 235 минут для отправки сообщения
        private void SendRestartMessage(object state)
        {
            var api = (ICoreServerAPI)state;
            string message = "До перезапуска сервера осталось 5 минут!";
            SendMessageToServer(api, message);   // Отправить в чат сервера
            SendMessageToTelegram(message);     // Отправить в Telegram
        }

        public override void Dispose()
        {
            // Останавливаем таймер при уничтожении мода
            timer?.Dispose();
            httpClient?.Dispose();
            base.Dispose();
        }
    }
}
