namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO para pedido de redefinição de password.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Número de funcionário do utilizador.
        /// </summary>
        public int NumFuncionario { get; set; }

        /// <summary>
        /// Nova password em texto simples.
        /// </summary>
        public string NovaPassword { get; set; }
    }
}