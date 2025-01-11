using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace BotTG
{
    public class Deadmess : ModSystem
    {
        private const string BotToken = "YOUR_TOKEN";  // Замените на ваш токен
        private const string ChatId = "CHAT_ID";  // Замените на ID вашего канала
        private const string TelegramApiUrl = "https://api.telegram.org/bot";
        private HttpClient httpClient;

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            httpClient = new HttpClient();

            // Подписка только на событие смерти игрока
            api.Event.PlayerDeath += OnPlayerDeath;
        }

        // Обработчик события смерти игрока
        private void OnPlayerDeath(IServerPlayer player, DamageSource damageSource)
        {
            // Отправляем сообщение о смерти игрока в Telegram
            string text = $"{player.PlayerName} умер на сервере Vintage Story.";
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
