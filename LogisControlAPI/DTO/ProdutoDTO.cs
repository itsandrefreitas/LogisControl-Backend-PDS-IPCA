
namespace LogisControlAPI.DTO
{
    public partial class ProdutoDTO
    {
        public int ProdutoId { get; set; }

        public string Nome { get; set; } = null!;

        public int Quantidade { get; set; }

        public string Descricao { get; set; } = null!;

        public string CodInterno { get; set; } = null!;

        public double Preco { get; set; }

        public int? OrdemProducaoOrdemProdId { get; set; }

        public int? EncomendaItensEncomendaItensId { get; set; }
    }
}
