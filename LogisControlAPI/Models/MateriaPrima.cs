using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisControlAPI.Models;

public partial class MateriaPrima
{
    [Column("MateriaPrimaID")]
    public int MateriaPrimaId { get; set; }

    public string Nome { get; set; } = null!;

    public int Quantidade { get; set; }

    public string Descricao { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public string CodInterno { get; set; } = null!;

    public double Preco { get; set; }

    public virtual ICollection<MateriaPrimaProduto> MateriaPrimaProdutos { get; set; } = new List<MateriaPrimaProduto>();

    public virtual ICollection<NotaEncomendaItens> NotasEncomendaItem { get; set; } = new List<NotaEncomendaItens>();

    public virtual ICollection<OrcamentoItem> OrcamentosItem { get; set; } = new List<OrcamentoItem>();

    public virtual ICollection<ProdMateriais> ProdMateriais { get; set; } = new List<ProdMateriais>();
}
