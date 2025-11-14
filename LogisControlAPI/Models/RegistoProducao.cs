using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class RegistoProducao
{
    public int RegistoProducaoId { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime DataProducao { get; set; }

    public string? Observacoes { get; set; }

    public int UtilizadorUtilizadorId { get; set; }

    public int ProdutoProdutoId { get; set; }

    public int OrdemProducaoOrdemProdId { get; set; }

    public virtual OrdemProducao OrdemProducaoOrdemProd { get; set; } = null!;

    public virtual Produto ProdutoProduto { get; set; } = null!;

    public virtual Utilizador UtilizadorUtilizador { get; set; } = null!;
}
