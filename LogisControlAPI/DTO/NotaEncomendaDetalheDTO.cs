namespace LogisControlAPI.DTO
{
    public class NotaEncomendaDetalheDTO
    {
        public int NotaEncomendaId { get; set; }
        public DateTime DataEmissao { get; set; }
        public string Estado { get; set; }
        public double ValorTotal { get; set; }
        public int OrcamentoId { get; set; }
        public List<NotaEncomendaItemDTO> Itens { get; set; }
    }
}
