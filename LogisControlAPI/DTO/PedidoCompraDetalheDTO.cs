using System;
using System.Collections.Generic;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO que traz o detalhe completo de um pedido de compra, incluindo itens.
    /// </summary>
    public class PedidoCompraDetalheDTO
    {
        public int PedidoCompraId { get; set; }
        public string Descricao { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime DataAbertura { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string NomeUtilizador { get; set; } = null!;
        public List<ItemPedidoDetalheDTO> Itens { get; set; } = new();
    }
    public class ItemPedidoDetalheDTO
    {
        /// <summary>ID da matéria-prima.</summary>
        public int MateriaPrimaId { get; set; }

        /// <summary>Nome da matéria-prima.</summary>
        public string MateriaPrimaNome { get; set; } = null!;

        /// <summary>Quantidade encomendada.</summary>
        public int Quantidade { get; set; }
    }
}
