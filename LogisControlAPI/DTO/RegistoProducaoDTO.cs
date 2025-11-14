using LogisControlAPI.Models;

namespace LogisControlAPI.DTO
{
    public partial class RegistoProducaoDTO
    {
        public int RegistoProducaoId { get; set; }
        public string Estado { get; set; }
        public DateTime DataProducao { get; set; }
        public string? Observacoes { get; set; }
        public string NomeUtilizador { get; set; }
        public string NomeProduto { get; set; }
        public int OrdemProducaoOrdemProdId { get; set; }

    }
}
