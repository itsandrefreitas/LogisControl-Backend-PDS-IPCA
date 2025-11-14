namespace LogisControlAPI.Interfaces
{
    public interface IEmailSender
    {
        Task EnviarAsync(string destinatario, string assunto, string mensagem);
    }
}
