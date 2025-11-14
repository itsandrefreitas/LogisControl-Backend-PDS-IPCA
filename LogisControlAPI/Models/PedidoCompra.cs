using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models
{
    public partial class PedidoCompra
    {
        public int PedidoCompraId { get; set; }

        public string Descricao { get; set; } = null!;

        public string Estado { get; set; } = null!;

        public DateTime DataAbertura { get; set; }

        public DateTime? DataConclusao { get; set; }

        public int UtilizadorUtilizadorId { get; set; }
        public virtual Utilizador UtilizadorUtilizador { get; set; } = null!;

        public virtual ICollection<PedidoCompraItem> PedidoCompraItems { get; set; }
            = new HashSet<PedidoCompraItem>();

        public virtual ICollection<PedidoCotacao> PedidosCotacao { get; set; }
            = new HashSet<PedidoCotacao>();
    }
}
