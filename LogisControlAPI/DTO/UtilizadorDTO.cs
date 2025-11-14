namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO para representar um utilizador sem informações sensíveis.
    /// </summary>
    public class UtilizadorDTO
    {
        public int UtilizadorId { get; set; }
        public string PrimeiroNome { get; set; } = null!;
        public string Sobrenome { get; set; } = null!;
        public int NumFuncionario { get; set; }
        public string Role { get; set; } = null!;
        public bool Estado { get; set; }
    }
}