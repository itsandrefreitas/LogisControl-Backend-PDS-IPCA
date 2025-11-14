using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class Fornecedor
{
    public int FornecedorId { get; set; }

    public string Nome { get; set; } = null!;

    public int? Telefone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<PedidoCotacao> PedidosCotacao { get; set; } = new List<PedidoCotacao>();
}
