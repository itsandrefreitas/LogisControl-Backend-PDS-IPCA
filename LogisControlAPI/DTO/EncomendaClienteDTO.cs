namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO para leitura de encomendas com nome do cliente.
    /// </summary>
    public class EncomendaClienteDTO
    {
        public int EncomendaClienteId { get; set; }
        public DateTime DataEncomenda { get; set; }
        public string Estado { get; set; } = null!;
        public string NomeCliente { get; set; } = null!;
    }
}
