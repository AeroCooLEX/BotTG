using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Newtonsoft.Json;

namespace TelegramToVintagestory
{
    public class TelegramBotMod : ModSystem
    {
        private const string BotToken = "YOUR_TOKEN";  // Замените на ваш токен бота
        private const string ChatId = "CHAT_ID";  // ID чата, куда будут отправляться сообщения
        private const string TelegramApiUrl = "https://api.telegram.org/bot";
        private HttpClient httpClient;

        // Ссылка на серверное API
        private ICoreServerAPI api;

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            this.api = api; // Сохраняем ссылку на api

            httpClient = new HttpClient();

            // Запуск слушателя сообщений из Telegram
            Task.Run(() => ListenToTelegramMessages());
        }

        private async Task ListenToTelegramMessages()
        {
            string url = $"{TelegramApiUrl}{BotToken}/getUpdates";
            int lastUpdateId = 0;

            while (true)
            {
                try
                {
                    // Получаем обновления с Telegram
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    string content = await response.Content.ReadAsStringAsync();

                    // Десериализуем JSON-ответ
                    var updates = JsonConvert.DeserializeObject<TelegramUpdateResponse>(content);

                    foreach (var update in updates.Result)
                    {
                        // Если новое сообщение
                        if (update.UpdateId > lastUpdateId)
                        {
                            lastUpdateId = update.UpdateId;
                            string messageText = update.Message?.Text;

                            if (!string.IsNullOrEmpty(messageText))
                            {
                                // Пересылаем сообщение в чат Vintagestory
                                api.BroadcastMessageToAllGroups($"[Telegram] {messageText}", EnumChatType.Notification, null);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке сообщений Telegram: {ex.Message}");
                }

                // Пауза перед следующей проверкой
                await Task.Delay(1000);
            }
        }

        public override void Dispose()
        {
            httpClient?.Dispose();
            base.Dispose();
        }
    }

    // Классы для десериализации данных от Telegram API
    public class TelegramUpdateResponse
    {
        [JsonProperty("result")]
        public Update[] Result { get; set; }
    }

    public class Update
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }

        [JsonProperty("message")]
        public TelegramMessage Message { get; set; }
    }

    public class TelegramMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
