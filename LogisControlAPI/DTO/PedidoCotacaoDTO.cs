namespace LogisControlAPI.DTO
{
    public class PedidoCotacaoDTO
    {
        public int PedidoCotacaoID { get; set; }
        public string Descricao { get; set; } = null!;
        public DateTime Data { get; set; }
        public string Estado { get; set; } = null!;
        public int FornecedorID { get; set; }
        public string TokenAcesso { get; set; } = null!;
        public string? FornecedorNome { get; set; }

    }
}