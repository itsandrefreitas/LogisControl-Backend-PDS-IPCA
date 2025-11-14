namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO utilizado para atualizar o estado de uma encomenda.
    /// </summary>
    public class AtualizarEstadoEncomendaDTO
    {
        /// <summary>
        /// Novo estado da encomenda.
        /// </summary>
        public string Estado { get; set; } = null!;
    }
}
