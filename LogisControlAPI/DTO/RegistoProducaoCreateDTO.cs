using LogisControlAPI.Models;

namespace LogisControlAPI.DTO
{
    public partial class RegistoProducaoCreateDTO
    {
        public string Estado { get; set; }
        public string? Observacoes { get; set; }
        public int ProdutoProdutoId { get; set; }
        public int OrdemProducaoOrdemProdId { get; set; }
    }
}
