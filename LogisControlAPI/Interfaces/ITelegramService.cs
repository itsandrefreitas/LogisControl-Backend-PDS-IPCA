namespace LogisControlAPI.Interfaces
{
    public interface ITelegramService
    {
        Task EnviarMensagemAsync(string mensagem, string tipo = "Manutencao");
    }
}