namespace LogisControlAPI.DTO
{

    /// <summary>
    /// DTO para atualizar o perfil do utilizador (dados pessoais + password).
    /// </summary>
    public class UtilizadorUpdateDTO
    {
        public string? PrimeiroNome { get; set; }
        public string? Sobrenome { get; set; }
        public string? NovaPassword { get; set; }
    }
}