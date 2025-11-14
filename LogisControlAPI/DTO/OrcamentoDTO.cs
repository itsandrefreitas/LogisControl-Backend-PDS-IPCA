using System.Text.Json.Serialization;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// Cabeçalho de um orçamento
    /// </summary>
    public class OrcamentoDTO
    {
        public int OrcamentoID { get; set; }
        public DateTime Data { get; set; }
        public string Estado { get; set; } = null!;

        
        public int PedidoCotacaoID { get; set; }
    }
}