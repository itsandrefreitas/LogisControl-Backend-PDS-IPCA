namespace LogisControlAPI.DTO
{
    public class OrcamentoDetalheDTO
    {
        public int OrcamentoID { get; set; }
        public int PedidoCotacaoID { get; set; }
        public DateTime Data { get; set; }
        public string Estado { get; set; }
        public List<OrcamentoItemDetalheDTO> Itens { get; set; }
    }
}