using LogisControlAPI.Interfaces;

namespace LogisControlAPI.Services
{
    /// <summary>
    /// Serviço responsável por enviar notificações por email.
    /// Utiliza a abstração IEmailSender, permitindo flexibilidade na implementação.
    /// </summary>
    public class NotificationService
    {
        private readonly IEmailSender _emailSender;

        public NotificationService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// Envia uma notificação por email.
        /// </summary>
        /// <param name="destinatario">Endereço de email do destinatário.</param>
        /// <param name="assunto">Assunto do email.</param>
        /// <param name="mensagem">Corpo da mensagem.</param>
        public virtual async Task NotificarAsync(string destinatario, string assunto, string mensagem)
        {

            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("O destinatário é obrigatório.");

            if (string.IsNullOrWhiteSpace(assunto))
                throw new ArgumentException("O assunto é obrigatório.");

            if (string.IsNullOrWhiteSpace(mensagem))
                throw new ArgumentException("A mensagem é obrigatória.");


            await _emailSender.EnviarAsync(destinatario, assunto, mensagem);
        }
    }
}
