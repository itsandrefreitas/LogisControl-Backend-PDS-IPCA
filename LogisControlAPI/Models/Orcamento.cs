using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class Orcamento
{
    public int OrcamentoID { get; set; }
    public DateTime Data { get; set; }
    public string Estado { get; set; } = null!;
    // FK para PedidoCotacao
    public int PedidoCotacaoPedidoCotacaoID { get; set; }

    // Navegações
    public virtual PedidoCotacao PedidoCotacaoPedidoCotacao { get; set; } = null!;
    public virtual ICollection<OrcamentoItem> OrcamentoItems { get; set; } = new List<OrcamentoItem>();
}