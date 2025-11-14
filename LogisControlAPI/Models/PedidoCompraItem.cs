using System.ComponentModel.DataAnnotations.Schema;

namespace LogisControlAPI.Models
{
    /// <summary>
    /// Entidade que representa cada linha de um pedido de compra.
    /// </summary>
    public class PedidoCompraItem
    {
        public int PedidoCompraItemId { get; set; }

        // FK para o cabeçalho PedidoCompra
        public int PedidoCompraId { get; set; }
        [ForeignKey(nameof(PedidoCompraId))]
        public PedidoCompra PedidoCompra { get; set; } = null!;

        // FK para a Matéria-Prima
        public int MateriaPrimaId { get; set; }
        [ForeignKey(nameof(MateriaPrimaId))]
        public MateriaPrima MateriaPrima { get; set; } = null!;

        // Quantidade pedida
        public int Quantidade { get; set; }
    }
}
