using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class ProdMateriais
{
    public int ProdMateriaisId { get; set; }

    public int QuantidadeUtilizada { get; set; }

    public int OrdemProducaoOrdemProdId { get; set; }

    public int MateriaPrimaMateriaPrimaId { get; set; }

    public virtual MateriaPrima MateriaPrimaMateriaPrimaIDNavigation { get; set; } = null!;

    public virtual OrdemProducao OrdemProducaoOrdemProd { get; set; } = null!;
}
