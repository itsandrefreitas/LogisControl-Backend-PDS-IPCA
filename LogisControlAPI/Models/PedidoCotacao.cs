using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class PedidoCotacao
{
    public int PedidoCotacaoId { get; set; }
    public string Descricao { get; set; } = null!;
    public DateTime Data { get; set; }
    public string Estado { get; set; } = null!;
    public string TokenAcesso { get; set; } = null!;

    // FK obrigatória para fornecedor
    public int FornecedorId { get; set; }
    public virtual Fornecedor Fornecedor { get; set; } = null!;

    // NOVO: FK opcional para PedidoCompra
    public int? PedidoCompraId { get; set; }
    public virtual PedidoCompra? PedidoCompra { get; set; }

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
}