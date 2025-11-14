using System;

namespace LogisControlAPI.DTO
{
    public class PedidoCompraDTO
    {
        public int PedidoCompraId { get; set; }
        public string Descricao { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime DataAbertura { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string NomeUtilizador { get; set; } = null!;
    }
}