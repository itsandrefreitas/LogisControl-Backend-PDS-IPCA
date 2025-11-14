namespace LogisControlAPI.DTO
{
    public class PedidoCotacaoDetalhadoDTO
    {
        public PedidoCotacaoDTO Header { get; set; } = null!;
        public List<OrcamentoDTO> Orcamentos { get; set; } = new();
        public List<OrcamentoItemDTO> Itens { get; set; } = new();
    }
}
