using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class MateriaPrimaProduto
{
    public int MateriaPrimaProdutoId { get; set; }

    public int QuantidadeNec { get; set; }

    public int MateriaPrimaMateriaPrimaId { get; set; }

    public int ProdutoProdutoId { get; set; }

    public virtual MateriaPrima MateriaPrimaMateriaPrimaIDNavigation { get; set; } = null!;

    public virtual Produto ProdutoProduto { get; set; } = null!;
}
