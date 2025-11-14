using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class EncomendaItens
{
    public int EncomendaItensId { get; set; }

    public int? Quantidade { get; set; }

    public int EncomendaClienteEncomendaClienteId { get; set; }

    public virtual EncomendaCliente EncomendaClienteEncomendaCliente { get; set; } = null!;
    public int ProdutoId { get; set; } // ou int? se for nullable
    public virtual Produto Produto { get; set; } = null!; // ou Produto? se for nullable


}
