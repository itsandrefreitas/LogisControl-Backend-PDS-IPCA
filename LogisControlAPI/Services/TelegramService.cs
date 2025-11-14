using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogisControlAPI.Auxiliar;
using LogisControlAPI.Interfaces;


namespace LogisControlAPI.Services
{
    /// <summary>
    /// Serviço para envio de notificações via Telegram.
    /// </summary>
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramConfig _config;

        public TelegramService(HttpClient httpClient, TelegramConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <summary>
        /// Envia uma mensagem para todos os chats definidos.
        /// </summary>
        /// <param name="mensagem">Texto da mensagem.</param>
        public async Task EnviarMensagemAsync(string mensagem, string tipo = "Manutencao")
        {
            if (!_config.ChatIds.TryGetValue(tipo, out var chatId))
                throw new Exception($"ChatId para o tipo '{tipo}' não foi encontrado.");

            var url = $"https://api.telegram.org/bot{_config.BotToken}/sendMessage";

            var conteudo = new Dictionary<string, string>
            {
                ["chat_id"] = chatId,
                ["text"] = mensagem
            };

            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(conteudo));
            response.EnsureSuccessStatusCode();
        }

    }
}
